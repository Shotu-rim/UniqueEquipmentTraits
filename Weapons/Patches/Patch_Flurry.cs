using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Tracks melee attack attempts for both Flurry and Tailwind.
    /// Consumes charges only when an attack reaches its resolution stage
    /// and maintains a thread-local stack for nested attack calls.
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Patch_Flurry_Attempt
    {
        internal sealed class Attempt
        {
            // Flurry effect - attacker
            internal CompWeaponTrait Comp;
            internal Pawn Pawn;
            internal bool WasActive;

            // Tailwind effect - defender
            internal CompWeaponTrait DefenderComp;
            internal Pawn Defender;
            internal bool TailwindWasActive;

            internal bool ReachedResolution;
        }

        [ThreadStatic]
        private static Stack<Attempt> attacks;

        internal static Attempt Current =>
            attacks != null && attacks.Count > 0
                ? attacks.Peek()
                : null;

        // Shared attack lifecycle
        public static void BeginAttempt(
            Verb_MeleeAttack attack,
            out object state)
        {
            state = null;

            Pawn attacker = attack.CasterPawn;

            CompWeaponTrait attackerComp =
                (attack.EquipmentSource as ThingWithComps)?
                    .TryGetComp<CompWeaponTrait>();

            Pawn defender = attack.CurrentTarget.Pawn;

            CompWeaponTrait defenderComp =
                (defender?.equipment?.Primary as ThingWithComps)?
                    .TryGetComp<CompWeaponTrait>();

            Attempt attempt = new Attempt
            {
                Comp = attackerComp,
                Pawn = attacker,
                WasActive = attackerComp != null &&
                            attackerComp.FlurryActiveFor(attacker),

                Defender = defender,
                DefenderComp = defenderComp,
                TailwindWasActive = defenderComp != null &&
                                    defenderComp.TailwindActiveFor(defender)
            };

            if (attacks == null)
                attacks = new Stack<Attempt>();

            attacks.Push(attempt);
            state = attempt;
        }

        public static void MarkResolved()
        {
            Attempt attempt = Current;

            if (attempt != null)
                attempt.ReachedResolution = true;
        }

        public static void ConsumeAttempt(object state)
        {
            Attempt attempt = state as Attempt;

            if (attempt == null ||
                !attempt.ReachedResolution)
                return;

            // Consume one Flurry charge
            if (attempt.WasActive &&
                attempt.Comp != null)
            {
                attempt.Comp.ConsumeFlurryAttempt(
                    attempt.Pawn);
            }

            // Consume one Tailwind charge
            if (attempt.TailwindWasActive &&
                attempt.DefenderComp != null)
            {
                attempt.DefenderComp.ConsumeTailwindAttempt(
                    attempt.Defender);
            }
        }

        public static void EndAttempt(object state)
        {
            if (state == null ||
                attacks == null ||
                attacks.Count == 0)
                return;

            if (ReferenceEquals(attacks.Peek(), state))
                attacks.Pop();
        }

        // Vanilla Harmony patches
        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttack __instance,
            out object __state)
        {
            BeginAttempt(__instance, out __state);
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand is MethodInfo method &&
                    method.Name == "GetNonMissChance" &&
                    method.DeclaringType != null &&
                    typeof(Verb_MeleeAttack).IsAssignableFrom(
                        method.DeclaringType))
                {
                    found = true;

                    yield return new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(Patch_Flurry_Attempt),
                            nameof(MarkResolved)));
                }

                yield return instruction;
            }

            if (!found)
            {
                Log.Error(
                    "[UET] Flurry: TryCastShot resolution marker " +
                    "not found; charges will not be consumed. " +
                    "Check RimWorld build.");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(object __state)
        {
            ConsumeAttempt(__state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            object __state)
        {
            EndAttempt(__state);

            return __exception;
        }
    }

    /// <summary>
    /// Scales damage generated by melee attacks while Flurry was active
    /// at the beginning of the current attack attempt.
    /// </summary>
    [HarmonyPatch(
        typeof(Verb_MeleeAttackDamage),
        "DamageInfosToApply")]
    public static class Patch_Flurry_Damage
    {
        public static IEnumerable<DamageInfo> ApplyDamageMultiplier(
            IEnumerable<DamageInfo> source)
        {
            var attempt = Patch_Flurry_Attempt.Current;

            if (attempt == null ||
                !attempt.WasActive ||
                attempt.Comp == null ||
                source == null)
            {
                return source;
            }

            return Scale(
                source,
                attempt.Comp.FlurryDamageMultiplier());
        }

        private static IEnumerable<DamageInfo> Scale(
            IEnumerable<DamageInfo> source,
            float factor)
        {
            foreach (DamageInfo info in source)
            {
                DamageInfo scaled = info;
                scaled.SetAmount(info.Amount * factor);

                yield return scaled;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(
            ref IEnumerable<DamageInfo> __result)
        {
            __result = ApplyDamageMultiplier(__result);
        }
    }


    /// <summary>
    /// Adjusts attack cooldowns for Flurry and Oppression.
    /// Flurry affects melee attacks only.
    /// Oppression affects both melee and ranged attacks.
    /// </summary>
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    internal static class Patch_Flurry_Cooldown
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo cooldownMethod = AccessTools.Method(
                typeof(VerbProperties),
                "AdjustedCooldownTicks",
                new Type[] { typeof(Verb), typeof(Pawn) });

            MethodInfo adjustMethod = AccessTools.Method(
                typeof(Patch_Flurry_Cooldown),
                nameof(AdjustCooldown));

            if (cooldownMethod == null || adjustMethod == null)
            {
                Log.Error(
                    "[UET] Attack cooldown patch: required method not found.");

                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;

                yield break;
            }

            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(cooldownMethod))
                {
                    found = true;

                    yield return new CodeInstruction(
                        OpCodes.Ldarg_0);

                    yield return new CodeInstruction(
                        OpCodes.Call,
                        adjustMethod);
                }
            }

            if (!found)
            {
                Log.Error(
                    "[UET] Attack cooldown call not found in " +
                    "TryCastNextBurstShot.");
            }
        }

        private static int AdjustCooldown(
            int cooldown,
            Verb verb)
        {
            if (verb == null)
                return cooldown;

            Pawn pawn = verb.CasterPawn;

            if (pawn == null)
                return cooldown;

            float adjusted = cooldown;

            // Flurry: melee attacks only.
            if (verb is Verb_MeleeAttack)
            {
                CompWeaponTrait comp =
                    (verb.EquipmentSource as ThingWithComps)?
                        .TryGetComp<CompWeaponTrait>();

                if (comp != null &&
                    comp.FlurryActiveFor(pawn))
                {
                    float bonus =
                        comp.FlurrySpeedBonus();

                    if (bonus > 0f)
                    {
                        adjusted /= 1f + bonus;
                    }
                }
            }

            // Oppression: all pawn attacks.
            HediffDef oppressionDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Oppression");

            if (oppressionDef != null &&
                pawn.health != null)
            {
                UniqueEquipmentTraits.Effects.Hediff_Oppression
                    oppression =
                    pawn.health.hediffSet
                        .GetFirstHediffOfDef(oppressionDef)
                    as UniqueEquipmentTraits.Effects.Hediff_Oppression;

                if (oppression != null &&
                    oppression.Slowdown > 0f)
                {
                    float speedMultiplier =
                        Math.Max(
                            0.05f,
                            1f - oppression.Slowdown);

                    adjusted /= speedMultiplier;
                }
            }

            // Overdrive: melee and ranged attacks.
            float overdriveMultiplier =
                UniqueEquipmentTraits.Core.Armor
                    .ArmorTraitOverdriveSpeedUtility
                    .GetSpeedMultiplier(pawn);

            if (overdriveMultiplier > 1f)
            {
                adjusted /= overdriveMultiplier;
            }

            return Math.Max(
                1,
                (int)Math.Round(adjusted));
        }
    }

}

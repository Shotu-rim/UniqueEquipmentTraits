
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
    // Rejestruje rzeczywiście wykonane próby ataku.
    // TryCastShot może zwrócić false zarówno przy pudle,
    // jak i wtedy, gdy atak w ogóle nie został wykonany.
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    internal static class Patch_Flurry_Attempt
    {
        internal sealed class Attempt
        {
            // Flurry — atakujący
            internal CompWeaponTrait Comp;
            internal Pawn Pawn;
            internal bool WasActive;

            // Tailwind — obrońca
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


        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttack __instance,
            out Attempt __state)
        {
            Pawn attacker = __instance.CasterPawn;

            CompWeaponTrait attackerComp =
                (__instance.EquipmentSource as ThingWithComps)?
                    .TryGetComp<CompWeaponTrait>();

            Pawn defender = __instance.CurrentTarget.Pawn;

            CompWeaponTrait defenderComp =
                (defender?.equipment?.Primary as ThingWithComps)?
                    .TryGetComp<CompWeaponTrait>();

            __state = new Attempt
            {
                // Flurry
                Comp = attackerComp,
                Pawn = attacker,
                WasActive = attackerComp != null &&
                            attackerComp.FlurryActiveFor(attacker),

                // Tailwind
                Defender = defender,
                DefenderComp = defenderComp,
                TailwindWasActive = defenderComp != null &&
                                    defenderComp.TailwindActiveFor(defender)
            };

            if (attacks == null)
                attacks = new Stack<Attempt>();

            attacks.Push(__state);
        }


        // Oznaczamy próbę ataku dopiero po przejściu
        // przez wstępne warunki TryCastShot.
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

        private static void MarkResolved()
        {
            Attempt attempt = Current;

            if (attempt != null)
                attempt.ReachedResolution = true;
        }


        [HarmonyPostfix]
        private static void Postfix(Attempt __state)
        {
            if (__state == null || !__state.ReachedResolution)
                return;

            // Flurry: consume one charge for the attacker's attempt.
            if (__state.WasActive && __state.Comp != null)
            {
                __state.Comp.ConsumeFlurryAttempt(
                    __state.Pawn);
            }

            // Tailwind: consume one charge for the defender's attempt.
            if (__state.TailwindWasActive &&
                __state.DefenderComp != null)
            {
                __state.DefenderComp.ConsumeTailwindAttempt(
                    __state.Defender);
            }
        }


        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception)
        {
            if (attacks != null && attacks.Count > 0)
                attacks.Pop();

            return __exception;
        }
    }


    // Zmniejsza obrażenia kolejnych ataków
    // wykonywanych podczas aktywnego Flurry.
    [HarmonyPatch(
    typeof(Verb_MeleeAttackDamage),
    "DamageInfosToApply")]
    internal static class Patch_Flurry_Damage
    {
        [HarmonyPostfix]
        private static void Postfix(
            ref IEnumerable<DamageInfo> __result)
        {
            var attempt = Patch_Flurry_Attempt.Current;

            if (attempt == null ||
                !attempt.WasActive ||
                __result == null)
            {
                return;
            }

            __result = Scale(
                __result,
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
    
    }

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
                    "[UET Flurry] Cooldown patch: required method not found.");

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

                    // Stack after AdjustedCooldownTicks:
                    // int cooldown
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        adjustMethod);
                }
            }

            if (!found)
            {
                Log.Error(
                    "[UET Flurry] Cooldown call not found in " +
                    "TryCastNextBurstShot.");
            }
        }

        private static int AdjustCooldown(int cooldown, Verb verb)
        {
            if (!(verb is Verb_MeleeAttack))
                return cooldown;

            Pawn pawn = verb.CasterPawn;

            CompWeaponTrait comp =
                (verb.EquipmentSource as ThingWithComps)?
                    .TryGetComp<CompWeaponTrait>();

            if (comp == null || !comp.FlurryActiveFor(pawn))
                return cooldown;

            float bonus = comp.FlurrySpeedBonus();

            if (bonus <= 0f)
                return cooldown;

            int adjusted = Math.Max(
                1,
                (int)Math.Round(cooldown / (1f + bonus)));

            return adjusted;
        }
    }
}

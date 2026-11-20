
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    internal static class Patch_ArmorTraitAcceleration
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo cooldownMethod = AccessTools.Method(
                typeof(VerbProperties),
                "AdjustedCooldownTicks",
                new Type[]
                {
                    typeof(Verb),
                    typeof(Pawn)
                });

            MethodInfo adjustMethod = AccessTools.Method(
                typeof(Patch_ArmorTraitAcceleration),
                nameof(AdjustCooldown));

            if (cooldownMethod == null || adjustMethod == null)
            {
                Log.Error(
                    "[UET Acceleration] Required cooldown method not found.");

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

                    // Cooldown is already on the evaluation stack
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        adjustMethod);
                }
            }

            if (!found)
            {
                Log.Error(
                    "[UET Acceleration] Cooldown call not found.");
            }
        }

        private static int AdjustCooldown(int cooldown, Verb verb)
        {
            if (!(verb is Verb_MeleeAttack))
                return cooldown;

            Pawn pawn = verb.CasterPawn;

            if (pawn == null)
                return cooldown;

            float multiplier =
                ArmorTraitAccelerationUtility
                    .GetCooldownMultiplier(pawn);

            if (multiplier >= 1f)
                return cooldown;

            return Math.Max(
                1,
                (int)Math.Round(cooldown * multiplier));
        }
    }
}

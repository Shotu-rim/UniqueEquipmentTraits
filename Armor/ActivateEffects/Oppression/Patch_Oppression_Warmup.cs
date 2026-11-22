
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Patches
{

    [HarmonyPatch(
        typeof(Verb),
        nameof(Verb.TryStartCastOn),
        new Type[]
        {
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(bool),
        typeof(bool),
        typeof(bool),
        typeof(bool)
        })]
    internal static class Patch_Oppression_Warmup

    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo secondsToTicks = AccessTools.Method(
                typeof(GenTicks),
                nameof(GenTicks.SecondsToTicks),
                new Type[] { typeof(float) });

            MethodInfo adjustMethod = AccessTools.Method(
                typeof(Patch_Oppression_Warmup),
                nameof(AdjustWarmup));

            if (secondsToTicks == null || adjustMethod == null)
            {
                Log.Error(
                    "[UET Oppression] Warmup patch: required method not found.");

                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;

                yield break;
            }

            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(secondsToTicks))
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
                    "[UET Oppression] Warmup conversion not found in TryStartCastOn.");
            }
        }


        private static int AdjustWarmup(
            int ticks,
            Verb verb)
        {
            if (verb == null || ticks <= 0)
                return ticks;

            Pawn pawn = verb.CasterPawn;

            if (pawn == null || pawn.health == null)
                return ticks;

            float adjusted = ticks;

            HediffDef oppressionDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Oppression");

            if (oppressionDef != null)
            {
                Hediff_Oppression oppression =
                    pawn.health.hediffSet
                        .GetFirstHediffOfDef(oppressionDef)
                    as Hediff_Oppression;

                if (oppression != null &&
                    oppression.Slowdown > 0f)
                {
                    float multiplier = Math.Max(
                        0.05f,
                        1f - oppression.Slowdown);

                    adjusted /= multiplier;
                }
            }

            float overdriveMultiplier =
                UniqueEquipmentTraits.Core.Armor
                    .ArmorTraitOverdriveSpeedUtility
                    .GetSpeedMultiplier(pawn);

            adjusted /= overdriveMultiplier;

            return Math.Max(
                1,
                (int)Math.Round(adjusted));
        }

    }
}

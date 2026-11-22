
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    internal static class Patch_Oppression_Burst
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo burstGetter =
                AccessTools.PropertyGetter(
                    typeof(Verb),
                    "TicksBetweenBurstShots");

            MethodInfo adjustMethod =
                AccessTools.Method(
                    typeof(Patch_Oppression_Burst),
                    nameof(AdjustBurstInterval));

            if (burstGetter == null || adjustMethod == null)
            {
                Log.Error(
                    "[UET Oppression] Burst patch: required method not found.");

                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;

                yield break;
            }

            int patchedCalls = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(burstGetter))
                {
                    patchedCalls++;

                    // Original burst interval is on the stack.
                    yield return new CodeInstruction(
                        OpCodes.Ldarg_0);

                    yield return new CodeInstruction(
                        OpCodes.Call,
                        adjustMethod);
                }
            }

            if (patchedCalls != 2)
            {
                Log.Warning(
                    "[UET Oppression] Expected 2 burst interval calls, found " +
                    patchedCalls +
                    ". Check RimWorld build.");
            }
        }


        private static int AdjustBurstInterval(
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

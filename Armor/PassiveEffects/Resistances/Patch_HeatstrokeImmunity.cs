
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(HediffGiver_Heat),
        nameof(HediffGiver_Heat.OnIntervalPassed))]
    public static class Patch_HeatstrokeImmunity
    {
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Low)]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo original = AccessTools.Method(
                typeof(HealthUtility),
                nameof(HealthUtility.AdjustSeverity),
                new Type[]
                {
                    typeof(Pawn),
                    typeof(HediffDef),
                    typeof(float)
                });

            MethodInfo replacement = AccessTools.Method(
                typeof(Patch_HeatstrokeImmunity),
                nameof(AdjustHeatstrokeSeverity));

            int replacements = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                if (original != null &&
                    replacement != null &&
                    (instruction.opcode == OpCodes.Call ||
                     instruction.opcode == OpCodes.Callvirt) &&
                    Equals(instruction.operand, original))
                {
                    CodeInstruction modified =
                        new CodeInstruction(
                            OpCodes.Call,
                            replacement);

                    modified.labels.AddRange(instruction.labels);
                    modified.blocks.AddRange(instruction.blocks);

                    replacements++;

                    yield return modified;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (replacements > 0)
            {
                Log.Message(
                    "[UET Heat Immunity] Heatstroke IL patch installed. " +
                    "Replaced calls: " + replacements);
            }
            else
            {
                Log.Warning(
                    "[UET Heat Immunity] No AdjustSeverity call found. " +
                    "Heatstroke protection was NOT installed.");
            }
        }

        public static void AdjustHeatstrokeSeverity(
            Pawn pawn,
            HediffDef hediffDef,
            float severityOffset)
        {
            if (pawn != null &&
                severityOffset > 0f &&
                hediffDef != null &&
                hediffDef.defName == "Heatstroke")
            {
                float immunity =
                    ArmorTraitImmunityUtility.GetBestImmunity(
                        pawn,
                        "UET_HeatImmunity");

                severityOffset *= 1f - immunity;
            }

            HealthUtility.AdjustSeverity(
                pawn,
                hediffDef,
                severityOffset);
        }
    }
}

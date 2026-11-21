
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(HediffGiver_Hypothermia),
        nameof(HediffGiver_Hypothermia.OnIntervalPassed))]
    public static class Patch_HypothermiaImmunity
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo original = AccessTools.Method(
                typeof(HealthUtility),
                nameof(HealthUtility.AdjustSeverity),
                new[]
                {
                    typeof(Pawn),
                    typeof(HediffDef),
                    typeof(float)
                });

            MethodInfo replacement = AccessTools.Method(
                typeof(Patch_HypothermiaImmunity),
                nameof(AdjustHypothermiaSeverity));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(original))
                {
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        replacement);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static void AdjustHypothermiaSeverity(
            Pawn pawn,
            HediffDef hediffDef,
            float amount)
        {
            if (pawn != null && amount > 0f)
            {
                float immunity =
                    ArmorTraitImmunityUtility.GetBestImmunity(
                        pawn,
                        "UET_ColdImmunity");

                amount *= 1f - immunity;
            }

            if (amount != 0f)
            {
                HealthUtility.AdjustSeverity(
                    pawn,
                    hediffDef,
                    amount);
            }
        }
    }
}

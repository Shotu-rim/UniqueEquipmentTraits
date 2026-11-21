
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
        typeof(ToxicUtility),
        nameof(ToxicUtility.DoPawnToxicDamage))]
    public static class Patch_ToxicBuildupImmunity
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
                typeof(Patch_ToxicBuildupImmunity),
                nameof(AdjustToxicSeverity));

            int replaced = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(original))
                {
                    replaced++;

                    CodeInstruction patched =
                        new CodeInstruction(
                            OpCodes.Call,
                            replacement);

                    patched.labels.AddRange(instruction.labels);
                    patched.blocks.AddRange(instruction.blocks);

                    yield return patched;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (replaced != 1)
            {
                Log.Warning(
                    "[UET] Toxic Immunity: expected 1 " +
                    "AdjustSeverity call, found " + replaced);
            }
        }

        public static void AdjustToxicSeverity(
            Pawn pawn,
            HediffDef hediffDef,
            float amount)
        {
            if (pawn != null &&
                hediffDef == HediffDefOf.ToxicBuildup &&
                amount > 0f)
            {
                float immunity =
                    ArmorTraitImmunityUtility.GetBestImmunity(
                        pawn,
                        "UET_ToxicImmunity");

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

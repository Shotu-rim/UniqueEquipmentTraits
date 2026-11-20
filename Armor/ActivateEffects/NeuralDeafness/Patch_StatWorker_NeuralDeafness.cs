
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(
    typeof(StatWorker),
    nameof(StatWorker.GetValue),
    new[] { typeof(StatRequest), typeof(bool) }
)]
    public static class Patch_StatWorker_NeuralDeafness
    {
        private static readonly AccessTools.FieldRef<
            StatWorker, StatDef> StatField =
            AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");

        public static void Postfix(
            StatWorker __instance,
            StatRequest req,
            ref float __result)
        {
            if (StatField(__instance) != StatDefOf.PsychicSensitivity)
                return;

            Pawn pawn = req.Thing as Pawn;

            if (pawn == null)
                return;

            bool hasPassive =
            ArmorTraitNeuralDeafnessUtility.HasNeuralDeafness(pawn);

            bool hasSilence =
                pawn.health != null &&
                pawn.health.hediffSet != null &&
                pawn.health.hediffSet.HasHediff(
                    DefDatabase<HediffDef>.GetNamed("UET_Silence"));

            if (hasPassive || hasSilence)
            {
                __result = 0f;
            }
        }
    }
}

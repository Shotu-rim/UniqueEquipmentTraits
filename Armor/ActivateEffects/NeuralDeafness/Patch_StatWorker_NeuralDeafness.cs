
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
            // Neural Deafness requires Royalty
            if (!ModsConfig.RoyaltyActive)
                return;
            if (StatField(__instance) != StatDefOf.PsychicSensitivity)
                return;

            Pawn pawn = req.Thing as Pawn;

            if (pawn == null)
                return;

            bool hasPassive =
            ArmorTraitNeuralDeafnessUtility.HasNeuralDeafness(pawn);


            HediffDef silenceDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Silence");

            bool hasSilence =
                silenceDef != null &&
                pawn.health != null &&
                pawn.health.hediffSet != null &&
                pawn.health.hediffSet.HasHediff(silenceDef);


            if (hasPassive || hasSilence)
            {
                __result = 0f;
            }
        }
    }
}

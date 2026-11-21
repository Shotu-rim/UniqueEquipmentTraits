
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Effects;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(
        typeof(Hediff_Injury),
        nameof(Hediff_Injury.TryMergeWith))]
    public static class Patch_ShreddingInjuryMerge
    {
        [HarmonyPostfix]
        public static void Postfix(
            Hediff_Injury __instance,
            Hediff other,
            bool __result)
        {
            // Only process successful injury merges.
            if (!__result ||
                __instance == null ||
                other == null)
                return;

            Pawn pawn = __instance.pawn;

            if (pawn == null ||
                pawn.health == null ||
                pawn.health.hediffSet == null)
                return;

            HediffDef shreddingDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_ShreddingBleeding");

            if (shreddingDef == null)
                return;

            Hediff_ShreddingBleeding shredding =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(shreddingDef)
                    as Hediff_ShreddingBleeding;

            if (shredding == null)
                return;

            shredding.TransferMergedInjury(
                __instance.loadID,
                other.loadID);

            pawn.health.Notify_HediffChanged(shredding);
        }
    }
}

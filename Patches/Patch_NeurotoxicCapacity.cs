
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits
{
    [HarmonyPatch(
        typeof(PawnCapacityUtility),
        nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    public static class Patch_NeurotoxicCapacity
    {
        [HarmonyPostfix]
        public static void Postfix(
            HediffSet diffSet,
            PawnCapacityDef capacity,
            List<PawnCapacityUtility.CapacityImpactor> impactors,
            bool forTradePrice,
            ref float __result)
        {
            if (diffSet == null || capacity == null)
                return;

            float weakening = 0f;
            Hediff_Neurotoxic representative = null;

            foreach (Hediff hediff in diffSet.hediffs)
            {
                if (hediff is Hediff_Neurotoxic neurotoxic &&
                    neurotoxic.AffectedCapacity == capacity)
                {
                    weakening += neurotoxic.Weakening;

                    if (representative == null)
                    {
                        representative = neurotoxic;
                    }
                }
            }

            if (weakening <= 0f)
                return;

            weakening = UnityEngine.Mathf.Clamp(
                weakening, 0f, 0.60f);

            // Add Hediff to Overview
            if (impactors != null)
            {
                impactors.Insert(
                    0,
                    new CapacityImpactorNeurotoxic
                    {
                        hediff = representative,
                        TotalWeakening = weakening
                    });
            }

            // Weaken is working even if no impactors
            __result *= 1f - weakening;
            __result = GenMath.RoundedHundredth(__result);
        }
    }
}

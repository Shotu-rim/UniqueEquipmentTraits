
using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits
{
    /// <summary>
    /// Applies accumulated neurotoxic weakening to pawn capacities.
    /// Combines weakening from all neurotoxic hediffs affecting
    /// the same capacity, limits the total reduction to 60%,
    /// and adds a consolidated capacity impactor for display.
    /// </summary>
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

            // Add Hediff to Pawn - degradation
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

            // Weaken added imdependently
            __result *= 1f - weakening;
            __result = GenMath.RoundedHundredth(__result);
        }
    }
}

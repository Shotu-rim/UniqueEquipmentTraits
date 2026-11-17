
using HarmonyLib;
using System.Collections.Generic;
using UniqueEquipmentTraits.Implants;
using UniqueEquipmentTraits.Mechanoids;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Consolidates nanite mechanical degradation impactors into a
    /// single entry in the pawn capacity impactor list.
    /// Preserves references to individual degradation hediffs
    /// while preventing multiple separate entries from appearing
    /// in the capacity breakdown.
    /// </summary>
    [HarmonyPatch(
        typeof(PawnCapacityUtility),
        nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    public static class Patch_NaniteCapacityDisplay
    {
        [HarmonyPostfix]
        public static void Postfix(
            List<PawnCapacityUtility.CapacityImpactor> impactors)
        {
            if (impactors == null)
                return;

            var combined =
                new CapacityImpactorNaniteDegradation();

            int firstIndex = -1;

            for (int i = impactors.Count - 1; i >= 0; i--)
            {
                var entry = impactors[i] as
                    PawnCapacityUtility.CapacityImpactorHediff;

                if (entry == null)
                    continue;

                var degradation = entry.hediff as
                    Hediff_NaniteMechanicalDegradation;

                if (degradation == null)
                    continue;

                firstIndex = i;

                if (!combined.degradations.Contains(degradation))
                {
                    combined.degradations.Insert(
                        0, degradation);
                }

                impactors.RemoveAt(i);
            }

            if (firstIndex < 0)
                return;

            combined.hediff = combined.degradations[0];

            impactors.Insert(firstIndex, combined);
        }
    }
}

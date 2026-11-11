
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Implants;
using UniqueEquipmentTraits.Mechanoids;

namespace UniqueEquipmentTraits.Patches
{
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

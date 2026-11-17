
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits
{
    /// <summary>
    /// Integrates neurotoxic treatment with RimWorld's tending system.
    /// Detects when glitterworld medicine is used to tend a selected
    /// neurotoxic hediff and applies an additional 10 percentage points
    /// of weakening reduction after the treatment completes.
    /// </summary>
    [HarmonyPatch(
        typeof(TendUtility),
        nameof(TendUtility.DoTend))]
    public static class Patch_NeurotoxicTending
    {
        public static void Prefix(
            Pawn patient,
            Medicine medicine,
            out bool __state)
        {
            __state = false;

            if (patient == null ||
                !patient.health.HasHediffsNeedingTend() ||
                medicine == null ||
                medicine.Destroyed ||
                medicine.def != ThingDefOf.MedicineUltratech)
            {
                return;
            }

            List<Hediff> selected = new List<Hediff>();

            TendUtility.GetOptimalHediffsToTendWithSingleTreatment(
                patient,
                true,
                selected);

            foreach (Hediff hediff in selected)
            {
                if (hediff is Hediff_Neurotoxic)
                {
                    __state = true;
                    return;
                }
            }
        }

        public static void Postfix(
            Pawn patient,
            bool __state)
        {
            if (!__state)
                return;

            const float treatmentStrength = 0.10f;

            NeurotoxicUtility.ApplyGlitterworldTreatment(
                patient,
                treatmentStrength);
        }
    }
}


using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits
{
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


using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class HemostasisEffectUtility
    {
        public static bool TryStopRandomBleeding(
            Pawn pawn,
            out string bodyPartName)
        {
            bodyPartName = null;

            if (pawn == null ||
                pawn.health == null ||
                pawn.health.hediffSet == null)
            {
                return false;
            }

            List<Hediff> candidates = new List<Hediff>();

            float highestBleedRate = 0f;

            const float tolerance = 0.00001f;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_Injury) &&
                    !(hediff is Hediff_MissingPart))
                {
                    continue;
                }

                float bleedRate = hediff.BleedRate;

                if (bleedRate <= 0f)
                    continue;

                HediffComp_Hemostasis comp =
                    hediff.TryGetComp<HediffComp_Hemostasis>();

                if (comp == null || comp.BleedingStopped)
                    continue;

                if (bleedRate > highestBleedRate + tolerance)
                {
                    highestBleedRate = bleedRate;

                    candidates.Clear();
                    candidates.Add(hediff);
                }
                else if (System.Math.Abs(
                    bleedRate - highestBleedRate) <= tolerance)
                {
                    candidates.Add(hediff);
                }
            }

            if (candidates.Count == 0)
                return false;

            // Random selection only among equally bleeding wounds
            Hediff selected = candidates.RandomElement();

            HediffComp_Hemostasis selectedComp =
                selected.TryGetComp<HediffComp_Hemostasis>();

            if (selectedComp == null ||
                !selectedComp.StopBleeding())
            {
                return false;
            }

            bodyPartName = selected.Part != null
                ? selected.Part.LabelCap
                : "unknown body part";

            return true;
        }
    }
}

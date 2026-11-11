
using System.Collections.Generic;
using System.Text;
using Verse;
using UniqueEquipmentTraits.Implants;

namespace UniqueEquipmentTraits.Implants
{
    public class CapacityImpactorNaniteDegradation
        : PawnCapacityUtility.CapacityImpactorHediff
    {
        public readonly List<Hediff> degradations =
            new List<Hediff>();

        public override string Readable(Pawn pawn)
        {
            StringBuilder text = new StringBuilder();

            text.Append("Nanite degradation:");

            foreach (Hediff degradation in degradations)
            {
                if (degradation == null)
                    continue;

                HediffComp_NaniteDamage comp =
                    degradation.TryGetComp<HediffComp_NaniteDamage>();

                float damage = comp != null
                    ? comp.AccumulatedDamage
                    : degradation.Severity;

                string partName = degradation.Part != null
                    ? degradation.Part.LabelCap
                    : "Unknown part";

                text.Append("\n    ");
                text.Append(partName);
                text.Append(": -");
                text.Append((damage * 100f).ToString("0.#"));
                text.Append("%");
            }

            return text.ToString();
        }
    }
}

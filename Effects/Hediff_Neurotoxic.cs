
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits
{
    public class Hediff_Neurotoxic : HediffWithComps
    {
        private PawnCapacityDef affectedCapacity;

        public PawnCapacityDef AffectedCapacity =>
            affectedCapacity;

        public float Weakening => Severity;

        public override string LabelBase
        {
            get
            {
                if (affectedCapacity == null)
                    return base.LabelBase;

                return $"{base.LabelBase} — {affectedCapacity.label}";
            }
        }

        public override string TipStringExtra
        {
            get
            {
                string vanillaTooltip = base.TipStringExtra;

                if (affectedCapacity == null)
                    return vanillaTooltip;

                string neurotoxicTooltip =
                    $"Affected capacity: {affectedCapacity.LabelCap}\n" +
                    $"Capacity reduction: {FormatWeakening(Severity)}";

                if (string.IsNullOrEmpty(vanillaTooltip))
                    return neurotoxicTooltip;

                return vanillaTooltip.TrimEnd() +
                    "\n\n" + neurotoxicTooltip;
            }
        }

        public static string FormatWeakening(float weakening)
        {
            return $"{weakening * 100f:F1}%";
        }

        public void Initialize(
            PawnCapacityDef capacity,
            float weakening)
        {
            affectedCapacity = capacity;
            Severity = weakening;
        }

        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(
                ref affectedCapacity,
                "affectedCapacity");
        }
    }

    public class CapacityImpactorNeurotoxic
        : PawnCapacityUtility.CapacityImpactorHediff
    {
        public float TotalWeakening;

        public override string Readable(Pawn pawn)
        {
            return "Neurotoxic: -" +
                Hediff_Neurotoxic.FormatWeakening(TotalWeakening);
        }
    }
}


using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_Exhausted : HediffWithComps
    {
        public int TicksRemaining;

        private float CapacityPenalty
        {
            get
            {
                return Severity >= 0.5f
                    ? 0.30f
                    : 0.40f;
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                return "-" +
                    (CapacityPenalty * 100f).ToString("0.#") +
                    "%, " +
                    (TicksRemaining / 60f).ToString("0.0") +
                    " s";
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (TicksRemaining <= 0)
            {
                if (pawn != null && pawn.health != null)
                    pawn.health.RemoveHediff(this);

                return;
            }

            TicksRemaining--;

            if (TicksRemaining <= 0 &&
                pawn != null &&
                pawn.health != null)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(
                ref TicksRemaining,
                "ticksRemaining",
                0);
        }
    }
}

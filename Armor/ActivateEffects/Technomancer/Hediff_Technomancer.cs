
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_Technomancer : HediffWithComps
    {
        public float CapacityBonus;
        public int TicksRemaining;

        public override string LabelInBrackets
        {
            get
            {
                return "+" +
                    (CapacityBonus * 100f).ToString("0.##") +
                    "%, " +
                    (TicksRemaining / 60f).ToString("0.0") +
                    " s";
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (TicksRemaining > 0)
                TicksRemaining--;

            if (TicksRemaining <= 0 &&
                pawn != null &&
                pawn.health != null)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public void Activate(
            float capacityBonus,
            int durationTicks)
        {
            CapacityBonus = capacityBonus;
            TicksRemaining = durationTicks;

            if (pawn != null && pawn.health != null)
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (pawn != null &&
                pawn.health != null)
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(
                ref CapacityBonus,
                "capacityBonus",
                0f);

            Scribe_Values.Look(
                ref TicksRemaining,
                "ticksRemaining",
                0);
        }
    }
}

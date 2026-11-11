
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_Crippling : Hediff
    {
        public float Slowdown;

        public override string LabelInBrackets
        {
            get
            {
                float seconds = TicksRemaining / 60f;

                return $"{Slowdown:P0}, {seconds:0.0} s";
            }
        }

        public int TicksRemaining;

        public override void Tick()
        {
            base.Tick();

            TicksRemaining--;

            if (TicksRemaining <= 0 && pawn != null)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public void ApplyCrippling(float slowdown, int durationTicks)
        {
            Slowdown = slowdown;
            TicksRemaining = durationTicks;

            if (pawn != null && pawn.health != null)
            {
                pawn.health.Notify_HediffChanged(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref Slowdown, "slowdown", 0f);
            Scribe_Values.Look(ref TicksRemaining, "ticksRemaining", 0);
        }
    }
}

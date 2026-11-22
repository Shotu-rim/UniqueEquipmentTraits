
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_Overdrive : HediffWithComps
    {
        public float SpeedBonus;
        public int TicksRemaining;
        public int ExhaustedTicks;
        public float ExhaustedPenalty;

        public override string LabelInBrackets
        {
            get
            {
                return "+" +
                    (SpeedBonus * 100f).ToString("0.#") +
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

        public override void PostRemoved()
        {
            Pawn owner = pawn;

            base.PostRemoved();

            if (owner == null ||
                owner.health == null ||
                owner.Dead ||
                ExhaustedTicks <= 0)
            {
                return;
            }

            HediffDef exhaustedDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Exhausted");

            if (exhaustedDef == null)
            {
                Log.Error(
                    "[UET Overdrive] Missing HediffDef: UET_Exhausted");
                return;
            }

            // Do not apply exhaustion twice.
            Hediff existing =
                owner.health.hediffSet
                    .GetFirstHediffOfDef(exhaustedDef);

            if (existing != null)
                return;

            Hediff_Exhausted exhausted =
                HediffMaker.MakeHediff(
                    exhaustedDef,
                    owner) as Hediff_Exhausted;

            if (exhausted == null)
            {
                Log.Error(
                    "[UET Overdrive] Invalid hediffClass for UET_Exhausted.");
                return;
            }

            // Select the XML stage.
            // Masterwork: -30 pp.
            // Legendary: -20 pp.
            exhausted.Severity =
                ExhaustedPenalty >= 0.40f
                    ? 0.1f
                    : 0.5f;

            exhausted.TicksRemaining = ExhaustedTicks;

            owner.health.AddHediff(exhausted);
        }

        public void Activate(
            float speedBonus,
            int durationTicks,
            float exhaustedPenalty,
            int exhaustedTicks)
        {
            SpeedBonus = speedBonus;
            TicksRemaining = durationTicks;
            ExhaustedPenalty = exhaustedPenalty;
            ExhaustedTicks = exhaustedTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(
                ref SpeedBonus,
                "speedBonus",
                0f);

            Scribe_Values.Look(
                ref TicksRemaining,
                "ticksRemaining",
                0);

            Scribe_Values.Look(
                ref ExhaustedTicks,
                "exhaustedTicks",
                0);

            Scribe_Values.Look(
                ref ExhaustedPenalty,
                "exhaustedPenalty",
                0f);
        }
    }
}

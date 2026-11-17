
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_SerratedBleeding : Hediff
    {
        public override float BleedRate
        {
            get
            {
                if (pawn == null ||
                    pawn.Dead ||
                    pawn.health == null ||
                    !pawn.health.CanBleed)
                    return 0f;

                return HasNaturalBleeding() ? Severity : 0f;
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn == null ||
                pawn.Dead ||
                pawn.health == null)
                return;

            if (!HasNaturalBleeding())
                pawn.health.RemoveHediff(this);
        }

        private bool HasNaturalBleeding()
        {
            if (pawn == null || pawn.health == null)
                return false;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff == this ||
                    hediff is Hediff_SerratedBleeding)
                    continue;

                if (hediff.BleedRate > 0f)
                    return true;
            }

            return false;
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (other == null ||
                other.def != def ||
                other.Part != Part)
                return false;

            Severity += other.Severity;
            ageTicks = 0;

            return true;
        }
    }
}

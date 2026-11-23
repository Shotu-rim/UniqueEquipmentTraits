
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class HediffCompProperties_Hemostasis : HediffCompProperties
    {
        public HediffCompProperties_Hemostasis()
        {
            compClass = typeof(HediffComp_Hemostasis);
        }
    }

    public class HediffComp_Hemostasis : HediffComp
    {
        private bool bleedingStopped;

        public bool BleedingStopped
        {
            get { return bleedingStopped; }
        }

        public bool StopBleeding()
        {
            if (bleedingStopped)
                return false;

            bleedingStopped = true;

            if (Pawn != null && Pawn.health != null &&
                Pawn.health.hediffSet != null)
            {
                Pawn.health.hediffSet.DirtyCache();
            }

            return true;
        }
        public void ResumeBleeding()
        {
            if (!bleedingStopped)
                return;

            bleedingStopped = false;

            if (Pawn != null &&
                Pawn.health != null &&
                Pawn.health.hediffSet != null)
            {
                Pawn.health.hediffSet.DirtyCache();
            }
        }

        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);

            if (!bleedingStopped || other == null)
                return;

            if (other.BleedRate > 0f)
                ResumeBleeding();
        }


        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(
                ref bleedingStopped,
                "uetHemostasisBleedingStopped",
                false);
        }
    }
}

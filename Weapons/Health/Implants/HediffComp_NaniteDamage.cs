
using Verse;

namespace UniqueEquipmentTraits.Implants
{
    public class HediffComp_NaniteDamage : HediffComp
    {
        private const float MaxDamage = 0.60f;

        private float accumulatedDamage = 0f;

        public float AccumulatedDamage => accumulatedDamage;

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            accumulatedDamage += amount;

            if (accumulatedDamage > MaxDamage)
            {
                accumulatedDamage = MaxDamage;
            }

            Pawn?.health?.capacities
                ?.Notify_CapacityLevelsDirty();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(
                ref accumulatedDamage,
                "uetNaniteAccumulatedDamage",
                0f
            );

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (accumulatedDamage < 0f)
                {
                    accumulatedDamage = 0f;
                }
                else if (accumulatedDamage > MaxDamage)
                {
                    accumulatedDamage = MaxDamage;
                }
            }
        }
    }
}

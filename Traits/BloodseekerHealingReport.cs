using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Traits
{
    public sealed class BloodseekerHealingReport
    {
        public sealed class PartInjuryInfo
        {
            public int WoundCount;
            public float MissingHP;
        }

        public float BleedingBeforeAttack;
        public float BleedingAfterAttack;
        public float BleedingUsed;
        public float BleedingFactor;

        public float EffectStrength;
        public float PartLimitFactor;

        public int InjuryCount;
        public float MissingHP;

        public float InitialHealingPool;
        public float TotalHealed;
        public float UnusedHealingPool;

        public string SkipReason;

        public readonly Dictionary<BodyPartRecord, PartInjuryInfo>
            InjuriesByPart =
                new Dictionary<BodyPartRecord, PartInjuryInfo>();

        public readonly Dictionary<BodyPartRecord, float>
            HealedByPart =
                new Dictionary<BodyPartRecord, float>();

        public bool HealingSucceeded => TotalHealed > 0f;
    }
}
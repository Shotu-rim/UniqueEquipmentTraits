
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public sealed class ArmorTraitActivationContext
    {
        public readonly Pawn Attacker;
        public readonly Pawn Defender;
        public readonly Apparel Apparel;
        public readonly ArmorTraitDef Trait;
        public readonly BodyPartRecord HitPart;
        public readonly int StackNumber;
        public readonly DamageInfo DamageInfo;
        public readonly float InjurySeverity;

        public ArmorTraitActivationContext(
            Pawn attacker,
            Pawn defender,
            Apparel apparel,
            ArmorTraitDef trait,
            BodyPartRecord hitPart,
            int stackNumber,
            DamageInfo damageInfo,
            float injurySeverity)
        {
            Attacker = attacker;
            Defender = defender;
            Apparel = apparel;
            Trait = trait;
            HitPart = hitPart;
            StackNumber = stackNumber;
            DamageInfo = damageInfo;
            InjurySeverity = injurySeverity;
        }
    }
}

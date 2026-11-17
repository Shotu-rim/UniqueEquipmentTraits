
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Holds combat and injury data for an activated reactive armor trait.
    /// Provides the attacker, defender, armor, trait, hit location, and damage details.
    /// Other mods can use this public context when implementing custom armor trait workers.
    /// </summary>
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

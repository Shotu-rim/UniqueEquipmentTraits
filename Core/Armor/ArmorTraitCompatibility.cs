
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitCompatibility
    {
        public static bool CanApply(
            ThingDef apparelDef,
            ArmorTraitDef trait)
        {
            if (apparelDef == null || trait == null)
                return false;

            if (!ArmorTraitEligibility.CanReceiveArmorTrait(
                    apparelDef))
            {
                return false;
            }

            return true;
        }
    }
}

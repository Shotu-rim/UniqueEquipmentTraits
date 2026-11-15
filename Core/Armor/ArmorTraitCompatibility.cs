
using System.Linq;
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

            // Brak ograniczeń oznacza uniwersalny trait.
            if (trait.allowedCategories == null ||
                trait.allowedCategories.Count == 0)
            {
                return true;
            }

            // Pancerz musi pokrywać przynajmniej jedną
            // z dozwolonych kategorii traitu.
            return ArmorTraitEligibility
                .GetCategories(apparelDef)
                .Any(category =>
                    trait.allowedCategories.Contains(category));
        }
    }
}

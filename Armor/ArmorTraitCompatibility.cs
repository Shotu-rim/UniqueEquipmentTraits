
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Checks whether an armor trait is compatible with a given apparel definition.
    /// Validates armor eligibility and optional anatomical category restrictions.
    /// Other mods can use CanApply() to verify trait compatibility before assignment.
    /// </summary>
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

            // No category = trait is universal
            if (trait.allowedCategories == null ||
                trait.allowedCategories.Count == 0)
            {
                return true;
            }

            // Armor must cover at least one category
            return ArmorTraitEligibility
                .GetCategories(apparelDef)
                .Any(category =>
                    trait.allowedCategories.Contains(category));
        }
    }
}

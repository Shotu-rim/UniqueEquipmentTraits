
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Checks armor trait compatibility with apparel definitions
    /// and individual apparel instances.
    /// Supports anatomical and material category restrictions.
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

            // No anatomical restrictions.
            if (trait.allowedCategories == null ||
                trait.allowedCategories.Count == 0)
            {
                return true;
            }

            // Armor must cover at least one allowed category.
            return ArmorTraitEligibility
                .GetCategories(apparelDef)
                .Any(category =>
                    trait.allowedCategories.Contains(category));
        }

        public static bool CanApply(
            ThingWithComps apparel,
            ArmorTraitDef trait)
        {
            if (apparel == null || trait == null)
                return false;

            // Validate general eligibility and anatomical coverage.
            if (!CanApply(apparel.def, trait))
                return false;

            // No material restrictions.
            if (trait.allowedStuffCategories == null ||
                trait.allowedStuffCategories.Count == 0)
            {
                return true;
            }

            // A material-restricted trait requires actual stuff.
            ThingDef stuff = apparel.Stuff;

            if (stuff == null || stuff.stuffProps == null ||
                stuff.stuffProps.categories == null)
            {
                return false;
            }

            // At least one material category must match.
            return stuff.stuffProps.categories.Any(category =>
                trait.allowedStuffCategories.Contains(category));
        }
    }
}

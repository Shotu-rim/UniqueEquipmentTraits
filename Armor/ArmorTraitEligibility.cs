using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Determines whether apparel is eligible for UET armor traits.
    /// Uses human anatomy to identify covered body categories.
    /// Other mods can use these methods to check apparel eligibility
    /// and retrieve its supported anatomical categories.
    /// </summary>
    public static class ArmorTraitEligibility
    {
        public static bool CanReceiveArmorTrait(ThingDef apparelDef)
        {
            return GetCategories(apparelDef).Count > 0;
        }

        public static List<ArmorTraitCategory> GetCategories(ThingDef apparelDef)
        {
            BodyDef humanBody = DefDatabase<BodyDef>.GetNamedSilentFail("Human");
            if (humanBody == null)
                return new List<ArmorTraitCategory>();

            return ArmorTraitCoverage.GetCoveredCategories(apparelDef, humanBody);
        }

        // Old signature for old code, in case, returns first eligible
        public static ArmorTraitCategory GetCategory(ThingDef apparelDef)
        {
            List<ArmorTraitCategory> categories = GetCategories(apparelDef);
            return categories.Count > 0 ? categories[0] : ArmorTraitCategory.None;
        }
    }
}

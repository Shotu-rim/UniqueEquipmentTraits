using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
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

        // Old signature for old code, in case
        public static ArmorTraitCategory GetCategory(ThingDef apparelDef)
        {
            List<ArmorTraitCategory> categories = GetCategories(apparelDef);
            return categories.Count > 0 ? categories[0] : ArmorTraitCategory.None;
        }
    }
}

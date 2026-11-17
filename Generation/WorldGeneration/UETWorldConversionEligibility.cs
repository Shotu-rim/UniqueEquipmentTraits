
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Determines whether an equipment item is eligible for UET world conversion.
    /// Accepts non-unique melee weapons and apparel of Masterwork or Legendary
    /// quality, while excluding items already containing CompUniqueWeapon.
    /// Performs preliminary eligibility checks without modifying the item.
    /// </summary>
    public static class UETWorldConversionEligibility
    {
        public static bool IsEligible(Thing thing)
        {
            if (thing == null || thing.def == null)
                return false;

            // Unique equipment unchanged
            if (thing.TryGetComp<CompUniqueWeapon>() != null)
                return false;

            ThingDef def = thing.def;

            // World conversion supports melee weapons and apparel
            if (!def.IsMeleeWeapon && !def.IsApparel)
                return false;

            // Only Masterwork and Legendary
            CompQuality quality =
                thing.TryGetComp<CompQuality>();

            if (quality == null)
                return false;

            if (quality.Quality != QualityCategory.Masterwork &&
                quality.Quality != QualityCategory.Legendary)
            {
                return false;
            }

            return true;
        }
    }
}

using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionEligibility
    {
        public static bool IsEligible(Thing thing)
        {
            if (thing == null || thing.def == null)
                return false;

            // Przedmioty już unikatowe pozostają nietknięte.
            if (thing.TryGetComp<CompUniqueWeapon>() != null)
                return false;

            ThingDef def = thing.def;

            // World Conversion: broń wręcz oraz pancerze.
            if (!def.IsMeleeWeapon && !def.IsApparel)
                return false;

            // Wymagana jakość Masterwork lub Legendary.
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
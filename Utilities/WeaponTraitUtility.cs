
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class WeaponTraitUtility
    {
        public static QualityCategory GetWeaponQuality(Thing weapon)
        {
            if (weapon == null)
                return QualityCategory.Normal;

            CompQuality qualityComp = weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return QualityCategory.Normal;

            return qualityComp.Quality;
        }

        public static bool HasQuality(Thing weapon)
        {
            if (weapon == null)
                return false;

            return weapon.TryGetComp<CompQuality>() != null;
        }
    }
}

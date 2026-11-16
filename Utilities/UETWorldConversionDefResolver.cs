
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionDefResolver
    {
        public static ThingDef Resolve(ThingDef original)
        {
            if (original == null)
                return null;

            ThingDef variant;

            // Broń wręcz.
            if (original.IsMeleeWeapon)
            {
                if (UniqueWeaponVariantRegistry.TryGetVariant(
                    original, out variant))
                {
                    return IsValidVariant(variant)
                        ? variant
                        : null;
                }

                return null;
            }

            // Pancerze i pozostałe kwalifikujące się apparel.
            if (original.IsApparel)
            {
                if (UniqueArmorVariantRegistry.TryGetVariant(
                    original, out variant))
                {
                    return IsValidVariant(variant)
                        ? variant
                        : null;
                }
            }

            return null;
        }

        private static bool IsValidVariant(ThingDef variant)
        {
            return variant != null &&
                   variant.HasComp<CompQuality>() &&
                   variant.HasComp<CompUniqueWeapon>();
        }
    }
}

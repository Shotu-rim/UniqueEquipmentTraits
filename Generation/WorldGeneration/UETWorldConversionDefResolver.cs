
using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Resolves registered UET unique variants for melee weapons
    /// and apparel during world equipment conversion.
    /// Uses the corresponding weapon or armor variant registry
    /// and validates required quality and unique weapon components.
    /// Returns null when no valid variant is available.
    /// </summary>
    public static class UETWorldConversionDefResolver
    {
        public static ThingDef Resolve(ThingDef original)
        {
            if (original == null)
                return null;

            ThingDef variant;

            // Resolve melee weapon variants
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

            // Resolve armor and other eligible apparel variants
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

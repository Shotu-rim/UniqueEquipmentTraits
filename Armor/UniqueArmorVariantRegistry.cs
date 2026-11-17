
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Maintains a registry mapping original apparel definitions to UET unique armor variants.
    /// Provides methods to register, retrieve, and clear variant mappings.
    /// Other mods can use TryGetVariant() to find generated variants
    /// and Register() to integrate their own compatible armor definitions.
    /// </summary>
    public static class UniqueArmorVariantRegistry
    {
        private static readonly Dictionary<ThingDef, ThingDef>
            variants = new Dictionary<ThingDef, ThingDef>();

        public static void Register(
            ThingDef original,
            ThingDef variant)
        {
            if (original == null || variant == null)
                return;

            variants[original] = variant;
        }

        public static bool TryGetVariant(
            ThingDef original,
            out ThingDef variant)
        {
            if (original == null)
            {
                variant = null;
                return false;
            }

            return variants.TryGetValue(
                original, out variant);
        }

        public static void Clear()
        {
            variants.Clear();
        }
    }
}

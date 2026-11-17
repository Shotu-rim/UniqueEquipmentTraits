
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Maintains a registry of original melee weapon definitions and
    /// their corresponding UET unique weapon variants.
    /// Provides variant lookup, registration with duplicate detection,
    /// and registry cleanup during definition generation.
    /// </summary>
    public static class UniqueWeaponVariantRegistry
    {
        private static readonly Dictionary<ThingDef, ThingDef>
            variants = new Dictionary<ThingDef, ThingDef>();

        public static bool TryGetVariant(
            ThingDef original,
            out ThingDef variant)
        {
            variant = null;

            if (original == null)
                return false;

            return variants.TryGetValue(
                original,
                out variant);
        }

        public static void Register(
            ThingDef original,
            ThingDef variant)
        {
            if (original == null || variant == null)
                return;

            if (!original.IsMeleeWeapon ||
                !variant.IsMeleeWeapon)
                return;

            if (original == variant)
                return;

            if (variants.ContainsKey(original))
            {
                Log.Warning(
                    "[UET] Duplicate unique variant for " +
                    original.defName);
                return;
            }

            variants.Add(original, variant);
        }

        public static void Clear()
        {
            variants.Clear();
        }
    }
}

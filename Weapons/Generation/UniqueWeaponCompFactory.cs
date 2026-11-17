
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Creates Odyssey unique weapon component properties for UET melee weapons.
    /// Assigns the UET_Melee weapon category and uses the original weapon's
    /// label as the basis for unique weapon naming.
    /// Returns null for invalid or non-melee weapons, or when the required
    /// weapon category definition is missing.
    /// </summary>
    public static class UniqueWeaponCompFactory
    {
        public static CompProperties_UniqueWeapon Create(
            ThingDef original)
        {
            if (original == null ||
                !original.IsMeleeWeapon)
            {
                return null;
            }

            WeaponCategoryDef category =
                DefDatabase<WeaponCategoryDef>
                    .GetNamedSilentFail("UET_Melee");

            if (category == null)
            {
                Log.Error(
                    "[UET] Missing weapon category: UET_Melee");

                return null;
            }

            CompProperties_UniqueWeapon properties =
                new CompProperties_UniqueWeapon();

            properties.weaponCategories =
                new List<WeaponCategoryDef>
                {
                    category
                };

            properties.namerLabels =
                new List<string>
                {
                    original.label ?? original.defName
                };

            return properties;
        }
    }
}

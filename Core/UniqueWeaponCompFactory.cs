
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
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

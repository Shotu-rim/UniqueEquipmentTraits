
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Provides access to registered weapon trait definitions.
    /// Allows enumeration of all WeaponTraitDef entries and
    /// lookup of individual traits by their definition name.
    /// </summary>
    public static class WeaponTraitDatabase
    {
        public static IEnumerable<WeaponTraitDef> AllTraits
        {
            get
            {
                return DefDatabase<WeaponTraitDef>.AllDefs;
            }
        }

        public static WeaponTraitDef GetTrait(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return null;

            return DefDatabase<WeaponTraitDef>
                .GetNamedSilentFail(defName);
        }
    }
}

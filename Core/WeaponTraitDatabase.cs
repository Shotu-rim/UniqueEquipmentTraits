
using System.Collections.Generic;
using Verse;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Core
{
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

            return DefDatabase<WeaponTraitDef>.GetNamedSilentFail(defName);
        }
    }
}

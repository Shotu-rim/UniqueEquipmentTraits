
using System.Collections.Generic;
using System.Linq;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Injects the UET weapon trait component into all registered
    /// melee weapon definitions, including weapons from other mods.
    /// Skips definitions that already contain the component to
    /// prevent duplicate injection.
    /// </summary>
    public static class WeaponTraitDefInjector
    {
        public static void Inject()
        {
            foreach (ThingDef weaponDef in
                DefDatabase<ThingDef>.AllDefs.ToList())
            {
                // Include all melee weapon, as long as done vanilla-like
                if (!weaponDef.IsMeleeWeapon)
                    continue;

                if (weaponDef.comps == null)
                    weaponDef.comps = new List<CompProperties>();

                if (weaponDef.HasComp<CompWeaponTrait>())
                    continue;

                weaponDef.comps.Add(
                    new CompProperties_WeaponTrait());
            }
        }
    }
}

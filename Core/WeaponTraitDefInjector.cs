
using System.Collections.Generic;
using System.Linq;
using Verse;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Core
{
    public static class WeaponTraitDefInjector
    {
        public static void Inject()
        {
            int injectedCount = 0;

            foreach (ThingDef weaponDef in
                DefDatabase<ThingDef>.AllDefs.ToList())
            {
                // For all melee weapons, including mods (if properly defined for Rimworld)
                if (!weaponDef.IsMeleeWeapon)
                    continue;

                if (weaponDef.comps == null)
                    weaponDef.comps = new List<CompProperties>();

                if (weaponDef.HasComp<CompWeaponTrait>())
                    continue;

                weaponDef.comps.Add(
                    new CompProperties_WeaponTrait());

                injectedCount++;
            }

            Log.Message(
                "[UET] Injected components: " +
                injectedCount);
        }
    }
}

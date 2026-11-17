
using Verse;

namespace UniqueEquipmentTraits.Traits
{
    /// <summary>
    /// Defines component properties for the UET weapon trait system.
    /// Associates these properties with CompWeaponTrait, allowing
    /// RimWorld to instantiate the component on equipment definitions
    /// that include it.
    /// </summary>
    public class CompProperties_WeaponTrait : CompProperties
    {
        public CompProperties_WeaponTrait()
        {
            compClass = typeof(CompWeaponTrait);
        }
    }
}

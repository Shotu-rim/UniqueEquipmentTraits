using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Manages equipment-related UET state cleanup.
    /// Clears active Flurry and Tailwind effects when a weapon
    /// with a CompWeaponTrait component is removed from a pawn.
    /// </summary>
    public static class UETEquipmentStateManager
    {

        public static void NotifyEquipmentRemoved(
            Pawn pawn,
            ThingWithComps equipment)
        {
            if (pawn == null || equipment == null)
                return;

            CompWeaponTrait comp =
                equipment.TryGetComp<CompWeaponTrait>();

            if (comp == null)
                return;

            comp.ClearFlurry();
            comp.ClearTailwind();
        }

    }
}

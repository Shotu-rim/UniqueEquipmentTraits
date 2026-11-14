
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Core
{
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

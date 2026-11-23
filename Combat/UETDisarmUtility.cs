
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class UETDisarmUtility
    {
        public static bool TryDisarm(
            Pawn victim,
            out ThingWithComps dropped)
        {
            dropped = null;

            if (victim == null ||
                victim.Dead ||
                victim.equipment == null ||
                !victim.Spawned)
            {
                return false;
            }

            ThingWithComps weapon = victim.equipment.Primary;

            if (weapon == null)
                return false;

            return victim.equipment.TryDropEquipment(
                weapon,
                out dropped,
                victim.Position,
                forbid: false) &&
                dropped != null;
        }
    }
}

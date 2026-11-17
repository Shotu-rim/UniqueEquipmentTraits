
using HarmonyLib;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Handles equipment removal notifications for the UET trait system.
    /// Delegates cleanup of weapon-bound trait states to
    /// UETEquipmentStateManager when equipment is removed from a pawn.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker),
        nameof(Pawn_EquipmentTracker.Notify_EquipmentRemoved))]
    public static class Patch_EquipmentRemoved
    {
        [HarmonyPostfix]
        public static void Postfix(
            Pawn_EquipmentTracker __instance,
            ThingWithComps eq)
        {
            if (__instance?.pawn == null || eq == null)
                return;

            UETEquipmentStateManager.NotifyEquipmentRemoved(
                __instance.pawn,
                eq);
        }
    }
}

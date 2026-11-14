
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Patches
{
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


//using HarmonyLib;
//using UniqueEquipmentTraits.Core;
//using Verse;

//namespace UniqueEquipmentTraits.Patches
//{
//    /// <summary>
//    /// Removes armor-bound effects when a pawn dies.
//    /// Does not modify persistent apparel state.
//    /// </summary>
//    [HarmonyPatch(
//        typeof(Pawn),
//        nameof(Pawn.Kill))]
//    public static class Patch_PawnDeathArmorCleanup
//    {
//        [HarmonyPrefix]
//        public static void Prefix(Pawn __instance)
//        {
//            if (__instance == null ||
//                __instance.Dead)
//            {
//                return;
//            }

//            UETEquipmentStateManager.NotifyWearerDied(
//                __instance);
//        }
//    }
//}

using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(
        typeof(Pawn),
        nameof(Pawn.Kill))]
    public static class Patch_PawnDeathArmorCleanup
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn __instance)
        {
            // TEMPORARY TEST:
            // Armor Hediff cleanup disabled during Pawn.Kill.
        }
    }
}

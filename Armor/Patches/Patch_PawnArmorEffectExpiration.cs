using HarmonyLib;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Periodically checks whether timed armor trait effects
    /// have expired and removes their associated hediffs.
    ///
    /// Runs after the pawn's tick update to avoid removing
    /// hediffs during the pawn's health update.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "TickInterval", new[] { typeof(int) })]
    public static class Patch_PawnArmorEffectExpiration
    {
        [HarmonyPostfix]
        public static void Postfix(
            Pawn __instance,
            int delta)
        {
            if (__instance == null ||
                Find.TickManager == null)
            {
                return;
            }

            // Check once every 60 ticks
            if (Find.TickManager.TicksGame % 60 != 0)
                return;

            UETEquipmentStateManager
                .CheckExpiredArmorEffects(__instance);
        }
    }
}
using HarmonyLib;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Removes expired armor trait hediffs from dead pawns
    /// during corpse rare ticks.
    /// </summary>
    [HarmonyPatch(typeof(Corpse), nameof(Corpse.TickRare))]
    public static class Patch_CorpseArmorEffectExpiration
    {
        [HarmonyPostfix]
        public static void Postfix(Corpse __instance)
        {
            if (__instance == null || __instance.Destroyed)
                return;

            Pawn pawn = __instance.InnerPawn;

            if (pawn == null || !pawn.Dead)
                return;

            UETEquipmentStateManager.CheckExpiredArmorEffects(pawn);
        }
    }
}
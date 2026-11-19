
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Applies passive armor trait bonuses to the
    /// vanilla neural heat recovery rate.
    /// </summary>
    [HarmonyPatch(
        typeof(Pawn_PsychicEntropyTracker),
        "get_RecoveryRate")]
    internal static class Patch_PsychicEntropyRecoveryRate
    {
        [HarmonyPostfix]
        private static void Postfix(
            Pawn_PsychicEntropyTracker __instance,
            ref float __result)
        {
            if (__instance == null ||
                __instance.Pawn == null)
            {
                return;
            }

            float bonus =
                ArmorTraitNeuralHeatUtility.GetRecoveryBonus(
                    __instance.Pawn);

            if (bonus <= 0f)
                return;

            __result *= 1f + bonus;
        }
    }
}

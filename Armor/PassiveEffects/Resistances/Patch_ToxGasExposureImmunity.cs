
using HarmonyLib;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(GasUtility),
        nameof(GasUtility.IsAffectedByExposure))]
    public static class Patch_ToxGasExposureImmunity
    {
        [HarmonyPostfix]
        public static void Postfix(
            Pawn pawn,
            ref bool __result)
        {
            // Preserve vanilla immunity.
            if (!__result || pawn == null)
                return;

            float immunity =
                ArmorTraitImmunityUtility.GetBestImmunity(
                    pawn,
                    "UET_ToxicImmunity");

            // Only complete immunity prevents exposure.
            if (immunity >= 1f)
                __result = false;
        }
    }
}

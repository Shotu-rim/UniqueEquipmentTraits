
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Patches natural immunity gain
    /// </summary>
    [HarmonyPatch(
        typeof(ImmunityRecord),
        nameof(ImmunityRecord.ImmunityChangePerTick))]
    internal static class Patch_ImmunityRecord_ImmunityChangePerTick
    {
        [HarmonyPostfix]
        private static void Postfix(
            Pawn pawn,
            bool sick,
            ref float __result)
        {
            if (pawn == null || !sick || __result <= 0f)
                return;

            float immunityGainBonus;
            float diseaseRiskReduction;

            ArmorTraitResilientUtility.GetBonuses(
                pawn,
                out immunityGainBonus,
                out diseaseRiskReduction);

            if (immunityGainBonus <= 0f)
                return;

            __result *= 1f + immunityGainBonus;
        }
    }
}

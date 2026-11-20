
using System.Reflection;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Patches natural disease resistance
    /// </summary>
    [HarmonyPatch]
    internal static class Patch_ImmunityHandler_DiseaseContractChanceFactor
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(ImmunityHandler),
                nameof(ImmunityHandler.DiseaseContractChanceFactor),
                new[]
                {
                    typeof(HediffDef),
                    typeof(HediffDef).MakeByRefType(),
                    typeof(BodyPartRecord)
                });
        }

        [HarmonyPostfix]
        private static void Postfix(
            ImmunityHandler __instance,
            ref float __result)
        {
            if (__instance == null ||
                __instance.pawn == null ||
                __result <= 0f)
                return;

            float immunityGainBonus;
            float diseaseRiskReduction;

            ArmorTraitResilientUtility.GetBonuses(
                __instance.pawn,
                out immunityGainBonus,
                out diseaseRiskReduction);

            if (diseaseRiskReduction <= 0f)
                return;

            __result *= 1f - diseaseRiskReduction;
        }
    }
}

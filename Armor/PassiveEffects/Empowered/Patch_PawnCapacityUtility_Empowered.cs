
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(
        typeof(PawnCapacityUtility),
        nameof(PawnCapacityUtility.CalculateCapacityLevel),
        new[]
        {
            typeof(HediffSet),
            typeof(PawnCapacityDef),
            typeof(List<PawnCapacityUtility.CapacityImpactor>),
            typeof(bool)
        })]
    [HarmonyAfter(new[]
    {
        "OskarPotocki.VEF"
    })]
    public static class Patch_PawnCapacityUtility_Empowered
    {
        public static void Postfix(
            HediffSet diffSet,
            PawnCapacityDef capacity,
            ref float __result)
        {
            if (diffSet == null ||
                diffSet.pawn == null ||
                capacity == null ||
                __result <= 0f)
                return;

            float bonus =
                ArmorTraitCapacityUtility.GetBestCapacityBonus(
                    diffSet.pawn);

            if (bonus <= 0f)
                return;

            __result = GenMath.RoundedHundredth(
                __result * (1f + bonus));
        }
    }
}

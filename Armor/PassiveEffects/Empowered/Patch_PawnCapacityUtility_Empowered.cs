
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Effects;
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
            {
                return;
            }

            Pawn pawn = diffSet.pawn;

            // Existing Empowered bonus
            float empoweredBonus =
                ArmorTraitCapacityUtility.GetBestCapacityBonus(
                    pawn);

            if (empoweredBonus > 0f)
            {
                __result *= 1f + empoweredBonus;
            }

            // Technomancer affects only Moving and Manipulation
            if (capacity == PawnCapacityDefOf.Moving ||
                capacity == PawnCapacityDefOf.Manipulation)
            {
                HediffDef technomancerDef =
                    DefDatabase<HediffDef>.GetNamedSilentFail(
                        "UET_Technomancer");

                if (technomancerDef != null)
                {
                    Hediff_Technomancer technomancer =
                        diffSet.GetFirstHediffOfDef(
                            technomancerDef)
                        as Hediff_Technomancer;

                    if (technomancer != null &&
                        technomancer.TicksRemaining > 0 &&
                        technomancer.CapacityBonus > 0f)
                    {
                        __result *=
                            1f + technomancer.CapacityBonus;
                    }
                }
            }

            __result =
                GenMath.RoundedHundredth(__result);
        }
    }
}

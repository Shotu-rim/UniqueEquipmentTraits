
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Hides UET armor trait Hediffs from the vanilla
    /// Health listing without removing them from pawns.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_HealthCardUtility_VisibleHediffs
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(HealthCardUtility),
                "VisibleHediffs",
                new Type[]
                {
                    typeof(Pawn),
                    typeof(bool)
                });
        }

        private static void Postfix(
            ref IEnumerable<Hediff> __result)
        {
            if (__result == null)
                return;

            __result = FilterHediffs(__result);
        }

        private static IEnumerable<Hediff> FilterHediffs(
            IEnumerable<Hediff> hediffs)
        {
            foreach (Hediff hediff in hediffs)
            {
                if (hediff == null)
                    continue;

                // Hide UET hediffs
                if (hediff.TryGetComp<
                    HediffComp_ArmorTraitSource>() != null)
                {
                    continue;
                }

                yield return hediff;
            }
        }
    }
}

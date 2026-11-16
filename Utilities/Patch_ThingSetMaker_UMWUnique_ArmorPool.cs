
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch]
    internal static class Patch_ThingSetMaker_UMWUnique_ArmorPool
    {
        [HarmonyPrepare]
        private static bool Prepare()
        {
            return AccessTools.TypeByName(
                "UniqueMeleeWeapons.ThingSetMaker_UMWUnique"
            ) != null;
        }

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName(
                "UniqueMeleeWeapons.ThingSetMaker_UMWUnique");

            if (type == null)
                return null;

            return AccessTools.Method(
                type,
                "AllGeneratableThingsDebugSub",
                new[] { typeof(ThingSetMakerParams) });
        }

        [HarmonyPostfix]
        private static void Postfix(
            ref IEnumerable<ThingDef> __result)
        {
            if (__result == null)
                return;

            List<ThingDef> candidates = __result.ToList();

            List<ThingDef> filtered = candidates
                .Where(UniqueArmorPoolFilter.CanGenerate)
                .ToList();

            int removed = candidates.Count - filtered.Count;

            if (Prefs.DevMode && removed > 0)
            {
                Log.Message(
                    "[UET][UMW][POOL] Removed " +
                    removed +
                    " UET armor variants");
            }

            __result = filtered;
        }
    }
}

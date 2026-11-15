
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(
        typeof(ThingSetMaker_MarketValue),
        "AllowedThingDefs")]
    public static class Patch_ThingSetMaker_MarketValue_AllowedThingDefs
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref IEnumerable<ThingDef> __result)
        {
            if (__result == null)
            {
                Log.Warning(
                    "[UET][WORLD][POOL] AllowedThingDefs returned NULL");
                return;
            }

            List<ThingDef> original = __result.ToList();

            if (Prefs.DevMode)
            {
                Log.Message(
                    "[UET][WORLD][POOL][START] " +
                    "Definitions: " + original.Count);
            }

            List<ThingDef> filtered =
                new List<ThingDef>(original.Count);

            int removed = 0;

            foreach (ThingDef def in original)
            {
                if (def == null)
                {
                    filtered.Add(def);
                    continue;
                }

                bool uetVariant =
                    def.defName != null &&
                    def.defName.StartsWith("UET_Unique_") &&
                    (def.defName.EndsWith("_Armor") ||
                     def.defName.EndsWith("_Weapon"));

                if (uetVariant)
                {
                    removed++;

                    if (Prefs.DevMode)
                    {
                        Log.Message(
                            "[UET][WORLD][POOL][REMOVE] " +
                            def.defName);
                    }

                    continue;
                }

                filtered.Add(def);
            }

            if (Prefs.DevMode)
            {
                Log.Message(
                    "[UET][WORLD][POOL][END] " +
                    "Original: " + original.Count +
                    " | Removed: " + removed +
                    " | Remaining: " + filtered.Count);
            }

            __result = filtered;
        }
    }
}

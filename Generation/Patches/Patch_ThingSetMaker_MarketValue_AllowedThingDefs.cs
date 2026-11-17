
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Excludes generated UET unique weapon and armor variants
    /// from the market-value-based ThingSetMaker definition pool.
    /// Prevents direct selection of generated variants while
    /// preserving normal equipment generation and subsequent
    /// UET world conversion.
    /// </summary>
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

            List<ThingDef> filtered =
                new List<ThingDef>(original.Count);

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
                    continue;

                filtered.Add(def);
            }

            __result = filtered;
        }
    }
}

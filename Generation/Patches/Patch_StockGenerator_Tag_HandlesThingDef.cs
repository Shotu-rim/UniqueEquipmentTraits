
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Excludes generated UET unique weapon and armor variants
    /// from tag-based trader stock generation.
    /// Prevents these variants from entering ordinary tagged
    /// stock pools without affecting other item definitions.
    /// </summary>
    [HarmonyPatch(
        typeof(StockGenerator_Tag),
        "HandlesThingDef")]
    public static class Patch_StockGenerator_Tag_HandlesThingDef
    {
        [HarmonyPostfix]
        private static void Postfix(
            ThingDef thingDef,
            ref bool __result)
        {
            if (!__result ||
                thingDef == null ||
                thingDef.defName == null)
            {
                return;
            }

            string name = thingDef.defName;

            if (name.StartsWith("UET_Unique_") &&
                (name.EndsWith("_Armor") ||
                 name.EndsWith("_Weapon")))
            {
                __result = false;
            }
        }
    }
}

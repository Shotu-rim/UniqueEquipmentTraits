
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Excludes generated UET unique weapon and armor variants
    /// from the miscellaneous trader stock generator.
    /// Prevents these variants from entering the normal miscellaneous
    /// stock pool while leaving other item definitions unaffected.
    /// </summary>
    [HarmonyPatch(
        typeof(StockGenerator_MiscItems),
        "HandlesThingDef")]
    public static class Patch_StockGenerator_MiscItems_HandlesThingDef
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


using System;
using System.Collections.Generic;
using HarmonyLib;
using LudeonTK;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(
        typeof(DebugThingPlaceHelper),
        nameof(DebugThingPlaceHelper.TryPlaceOptionsForStackCount))]
    public static class Patch_DebugThingPlaceHelper_HideUETVariants
    {
        private static void Postfix(
            ref List<DebugActionNode> __result)
        {
            if (__result == null)
                return;

            __result.RemoveAll(node =>
                node != null &&
                node.label != null &&
                node.label.StartsWith(
                    "UET_Unique_",
                    StringComparison.Ordinal));
        }
    }
}

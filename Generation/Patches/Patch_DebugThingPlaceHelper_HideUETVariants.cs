
using HarmonyLib;
using LudeonTK;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Hides generated UET unique equipment variants from the
    /// vanilla debug item placement menu.
    /// Filters entries by their UET_Unique_ label prefix without
    /// affecting normal item generation or dedicated UET dev tools.
    /// </summary>
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

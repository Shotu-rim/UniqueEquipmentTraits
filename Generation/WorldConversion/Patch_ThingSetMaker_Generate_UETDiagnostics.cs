
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Integrates UET world equipment conversion with ThingSetMaker.
    /// Tracks nested generation calls and processes the resulting
    /// equipment collection only at the outermost generation level.
    /// Prevents duplicate conversion passes during nested generation.
    /// </summary>
    [HarmonyPatch(
        typeof(ThingSetMaker),
        nameof(ThingSetMaker.Generate),
        new Type[] { typeof(ThingSetMakerParams) })]
    public static class Patch_ThingSetMaker_Generate_UETDiagnostics
    {
        [ThreadStatic]
        private static int worldConversionDepth;

        [HarmonyPrefix]
        private static void Prefix()
        {
            worldConversionDepth++;
        }

        [HarmonyPostfix]
        private static void Postfix(List<Thing> __result)
        {
            try
            {
                // Process last result
                if (worldConversionDepth == 1 &&
                    __result != null)
                {
                    UETWorldConversionProcessor.Process(__result);
                }
            }
            finally
            {
                worldConversionDepth--;
            }
        }
    }
}

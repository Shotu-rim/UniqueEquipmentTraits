
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using static UnityEngine.Scripting.GarbageCollector;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(
        typeof(ThingSetMaker),
        nameof(ThingSetMaker.Generate),
        new Type[] { typeof(ThingSetMakerParams) })]
    public static class Patch_ThingSetMaker_Generate_UETDiagnostics
    {
        private sealed class GeneratorStats
        {
            public int Calls;
            public int RootCalls;
            public int NestedCalls;
            public int Unique;
            public int Candidates;
            public int Skipped;
        }

        private sealed class GenerationContext
        {
            public readonly HashSet<Thing> Seen =
                new HashSet<Thing>();

            public readonly HashSet<Thing> Candidates =
                new HashSet<Thing>();
        }

        private sealed class CallState
        {
            public bool Active;
            public bool Root;
            public GenerationContext Context;
        }

        [ThreadStatic]
        private static int depth;

        [ThreadStatic]
        private static GenerationContext currentContext;
        [ThreadStatic]
        private static int worldConversionDepth;

        private static readonly Dictionary<string, GeneratorStats>
            statistics = new Dictionary<string, GeneratorStats>();

        private static int totalOperations;
        private static int totalUnique;
        private static int totalCandidates;
        private static int totalSkipped;

        [HarmonyPrefix]
        private static void Prefix(
            ThingSetMaker __instance,
            out CallState __state)
        {
            __state = new CallState();

            // World Conversion must track nesting even outside DevMode.
            worldConversionDepth++;

            if (!Prefs.DevMode)
                return;

            __state.Active = true;
            __state.Root = depth == 0;

            if (__state.Root)
            {
                currentContext = new GenerationContext();
                totalOperations++;
            }

            __state.Context = currentContext;
            depth++;

            string source = __instance.GetType().FullName;

            GeneratorStats stats = GetStats(source);

            stats.Calls++;

            if (__state.Root)
                stats.RootCalls++;
            else
                stats.NestedCalls++;
        }


        [HarmonyPostfix]
        private static void Postfix(
            ThingSetMaker __instance,
            List<Thing> __result,
            CallState __state)
        {
            try
            {
                // Only process the outermost Generate() call.
                if (worldConversionDepth == 1 &&
                    __result != null)
                {
                    UETWorldConversionProcessor.Process(__result);
                }

                // Existing diagnostics remain DevMode-only.
                if (__state != null && __state.Active)
                {
                    if (__result != null)
                    {
                        AnalyzeResults(
                            __instance.GetType().FullName,
                            __result,
                            __state.Context);
                    }
                }
            }
            finally
            {
                if (__state != null && __state.Active)
                {
                    depth--;

                    if (__state.Root)
                        currentContext = null;
                }

                worldConversionDepth--;
            }
        }


        private static void AnalyzeResults(
            string source,
            List<Thing> things,
            GenerationContext context)
        {
            GeneratorStats stats = GetStats(source);

            foreach (Thing original in things)
            {
                if (original == null)
                    continue;

                Thing thing = original;

                if (thing is MinifiedThing wrapper)
                    thing = wrapper.InnerThing;

                if (thing == null || thing.def == null)
                    continue;

                // Ten sam przedmiot może pojawić się
                // w wynikach kilku zagnieżdżonych generatorów.
                if (!context.Seen.Add(thing))
                    continue;

                CompUniqueWeapon unique =
                    thing.TryGetComp<CompUniqueWeapon>();

                if (unique != null)
                {
                    stats.Unique++;
                    totalUnique++;
                    continue;
                }

                ThingDef def = thing.def;

                if (!def.IsMeleeWeapon && !def.IsApparel)
                {
                    stats.Skipped++;
                    totalSkipped++;
                    continue;
                }

                CompQuality quality =
                    thing.TryGetComp<CompQuality>();

                if (quality == null ||
                    (quality.Quality != QualityCategory.Masterwork &&
                     quality.Quality != QualityCategory.Legendary))
                {
                    stats.Skipped++;
                    totalSkipped++;
                    continue;
                }

                context.Candidates.Add(thing);

                stats.Candidates++;
                totalCandidates++;

                Log.Message(
                    "[UET][WORLD][CANDIDATE] " +
                    "Source: " + source +
                    " | Def: " + def.defName +
                    " | Quality: " + quality.Quality +
                    " | Stuff: " +
                    (thing.Stuff != null
                        ? thing.Stuff.defName
                        : "none"));
            }
        }

        private static GeneratorStats GetStats(string source)
        {
            if (!statistics.TryGetValue(
                source, out GeneratorStats stats))
            {
                stats = new GeneratorStats();
                statistics.Add(source, stats);
            }

            return stats;
        }

        public static void PrintSummary()
        {
            Log.Message(
                "[UET][WORLD][SUMMARY] " +
                "RootOperations: " + totalOperations +
                " | AlreadyUnique: " + totalUnique +
                " | PreliminaryCandidates: " + totalCandidates +
                " | Skipped: " + totalSkipped);

            foreach (KeyValuePair<string, GeneratorStats> entry
                in statistics)
            {
                GeneratorStats stats = entry.Value;

                Log.Message(
                    "[UET][WORLD][GENERATOR] " +
                    entry.Key +
                    " | Calls: " + stats.Calls +
                    " | Root: " + stats.RootCalls +
                    " | Nested: " + stats.NestedCalls +
                    " | Unique: " + stats.Unique +
                    " | Candidates: " + stats.Candidates +
                    " | Skipped: " + stats.Skipped);
            }
        }

        public static void ResetStatistics()
        {
            statistics.Clear();

            totalOperations = 0;
            totalUnique = 0;
            totalCandidates = 0;
            totalSkipped = 0;
        }
    }
}

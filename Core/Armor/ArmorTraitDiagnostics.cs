
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitDiagnostics
    {
        private static readonly bool VerboseItemListing = false;
        public static void Run()
        {
            BodyDef humanBody =
                DefDatabase<BodyDef>.GetNamedSilentFail("Human");

            if (humanBody == null)
            {
                Log.Error("[UET] Human BodyDef not found.");
                return;
            }

            var counts =
                new Dictionary<ArmorTraitCategory, int>();

            foreach (ArmorTraitCategory category
                in Enum.GetValues(typeof(ArmorTraitCategory)))
            {
                counts[category] = 0;
            }

            int total = 0;
            int eligible = 0;

            Log.Message(
                "[UET] === Armor Coverage Diagnostics ===");

            foreach (ThingDef def
                in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.apparel == null)
                    continue;


                if (VerboseItemListing && (def.defName == "VFEM2_Shield_Heater" ||
                    def.defName == "VFEM2_Shield_Round" ||
                    def.defName == "VAE_Apparel_Shield_Riot" ||
                    def.defName == "MXIron_Shield" ||
                    def.defName == "Apparel_HeavyShield" ||
                    def.defName == "Apparel_ShieldBelt"))
                {
                    Log.Message(
                        "[UET SHIELD CHECK] " + def.defName +
                        " | ThingClass: " +
                        (def.thingClass?.FullName ?? "NULL") +
                        " | Layers: " +
                        FormatDefs(def.apparel.layers) +
                        " | Comps: " +
                        (def.comps == null
                            ? "-"
                            : string.Join(",",
                                def.comps
                                    .Where(c => c != null)
                                    .Select(c =>
                                        c.compClass?.FullName ?? "NULL")
                                    .ToArray())));
                }


                total++;

                List<ArmorTraitCategory> categories =
                    ArmorTraitCoverage.GetCoveredCategories(
                        def, humanBody);

                if (categories.Count == 0)
                {
                    counts[ArmorTraitCategory.None]++;
                }
                else
                {
                    eligible++;

                    foreach (ArmorTraitCategory category
                        in categories)
                    {
                        counts[category]++;
                    }
                }

                if (VerboseItemListing)
                    Log.Message(
                    "[UET] " + def.defName +
                    " | Layers: " +
                    FormatDefs(def.apparel.layers) +
                    " | BodyGroups: " +
                    FormatDefs(def.apparel.bodyPartGroups) +
                    " | UET Zones: " +
                    (categories.Count == 0
                        ? "None"
                        : string.Join(",",
                            categories.Select(c => c.ToString())
                                .ToArray())));
            }

            Log.Message("[UET] === Coverage Summary ===");
            Log.Message("[UET] Apparel total: " + total);
            Log.Message("[UET] With zones: " + eligible);
            Log.Message("[UET] Without zones: " +
                (total - eligible));

            foreach (var entry in counts)
            {
                Log.Message(
                    "[UET] " + entry.Key +
                    ": " + entry.Value);
            }
        }

        private static string FormatDefs<T>(
            IEnumerable<T> defs) where T : Def
        {
            if (defs == null)
                return "-";

            return string.Join(",",
                defs.Where(d => d != null)
                    .Select(d => d.defName)
                    .ToArray());
        }
    }
}

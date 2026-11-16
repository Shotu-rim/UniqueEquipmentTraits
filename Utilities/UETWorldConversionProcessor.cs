
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionProcessor
    {
        private const float MasterworkChance = 0.50f;
        private const float LegendaryChance = 0.40f;

        private const int ReportInterval = 10;

        private class ConversionStats
        {
            public int Eligible;
            public int RollPassed;
            public int Converted;
            public int Failed;
        }

        private static readonly ConversionStats masterworkWeapons =
            new ConversionStats();

        private static readonly ConversionStats masterworkArmor =
            new ConversionStats();

        private static readonly ConversionStats legendaryWeapons =
            new ConversionStats();

        private static readonly ConversionStats legendaryArmor =
            new ConversionStats();

        private static int totalRolls;

        public static void Process(List<Thing> things)
        {
            if (things == null || things.Count == 0)
                return;

            // One attempt per Thing instance within this list.
            HashSet<Thing> processed = new HashSet<Thing>();

            // Reuse the same replacement if the same source
            // reference occurs multiple times.
            Dictionary<Thing, Thing> replacements =
                new Dictionary<Thing, Thing>();

            for (int i = 0; i < things.Count; i++)
            {
                Thing original = things[i];

                if (original == null)
                    continue;

                if (replacements.TryGetValue(
                    original, out Thing existing))
                {
                    things[i] = existing;
                    continue;
                }

                // Minified items need separate handling.
                // Do not replace their inner Thing here.
                if (original is MinifiedThing)
                    continue;

                if (!processed.Add(original))
                    continue;

                if (!UETWorldConversionEligibility.IsEligible(original))
                    continue;

                CompQuality quality =
                    original.TryGetComp<CompQuality>();

                if (quality == null)
                    continue;

                float chance = GetChance(quality.Quality);

                if (Prefs.DevMode && original.def.IsMeleeWeapon)
                {
                    Log.Message(
                        "[UET][WORLD][WEAPON ROLL] " +
                        original.def.defName +
                        " | Quality: " + quality.Quality +
                        " | Chance: " + (chance * 100f) + "%");
                }

                // Only Masterwork and Legendary participate
                // in the conversion lottery.
                if (chance <= 0f)
                    continue;

                ConversionStats stats =
                    GetStats(original, quality.Quality);

                // Preserve the original random roll.
                bool rollPassed = Rand.Chance(chance);

                if (stats != null && Prefs.DevMode)
                {
                    stats.Eligible++;
                    totalRolls++;

                    if (rollPassed)
                        stats.RollPassed++;
                }

                if (rollPassed)
                {
                    if (!UETWorldConversionThingFactory.TryCreate(
                        original, out Thing converted))
                    {
                        if (stats != null && Prefs.DevMode)
                            stats.Failed++;
                    }
                    else
                    {
                        replacements.Add(original, converted);
                        things[i] = converted;

                        if (stats != null && Prefs.DevMode)
                            stats.Converted++;

                        if (Prefs.DevMode)
                        {
                            Log.Message(
                                "[UET][WORLD][CONVERTED] " +
                                original.def.defName +
                                " -> " + converted.def.defName +
                                " | Quality: " + quality.Quality);
                        }
                    }
                }

                if (Prefs.DevMode &&
                    totalRolls > 0 &&
                    totalRolls % ReportInterval == 0)
                {
                    PrintReport();
                }
            }
        }

        private static ConversionStats GetStats(
            Thing thing,
            QualityCategory quality)
        {
            bool weapon = thing.def.IsMeleeWeapon;
            bool armor = thing.def.IsApparel;

            if (!weapon && !armor)
                return null;

            if (quality == QualityCategory.Masterwork)
                return weapon
                    ? masterworkWeapons
                    : masterworkArmor;

            if (quality == QualityCategory.Legendary)
                return weapon
                    ? legendaryWeapons
                    : legendaryArmor;

            return null;
        }

        private static void PrintReport()
        {
            Log.Message(
                "\n[UET][WORLD][STATS]" +
                "\nTotal eligible rolls: " + totalRolls +
                "\nMasterwork (expected: 50%)" +
                FormatStats("Weapons", masterworkWeapons) +
                FormatStats("Armor", masterworkArmor) +
                "\nLegendary (expected: 40%)" +
                FormatStats("Weapons", legendaryWeapons) +
                FormatStats("Armor", legendaryArmor));
        }

        private static string FormatStats(
            string label,
            ConversionStats stats)
        {
            float rollRate = stats.Eligible > 0
                ? 100f * stats.RollPassed / stats.Eligible
                : 0f;

            float conversionRate = stats.Eligible > 0
                ? 100f * stats.Converted / stats.Eligible
                : 0f;

            return "\n  " + label +
                ": Eligible=" + stats.Eligible +
                " | Passed=" + stats.RollPassed +
                " | Converted=" + stats.Converted +
                " | Failed=" + stats.Failed +
                " | RollRate=" + rollRate.ToString("F1") + "%" +
                " | ConversionRate=" +
                conversionRate.ToString("F1") + "%";
        }

        private static float GetChance(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return MasterworkChance;

                case QualityCategory.Legendary:
                    return LegendaryChance;

                default:
                    return 0f;
            }
        }
    }
}

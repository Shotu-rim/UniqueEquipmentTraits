
using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitHitDiagnostics
    {
        private const int SampleCount = 10000;
        private const int SequenceSampleCount = 100000;

        public static void Run(Pawn pawn)
        {
            if (!ValidPawn(pawn))
            {
                Log.Warning("[UET HIT] Invalid pawn.");
                return;
            }

            DamageDef damage = DamageDefOf.Cut;
            var counts = NewCounts();
            int nullParts = 0;

            Rand.PushState();
            try
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    BodyPartRecord part =
                        pawn.health.hediffSet.GetRandomNotMissingPart(damage);

                    if (part == null)
                    {
                        nullParts++;
                        continue;
                    }

                    counts[ArmorTraitAnatomy.GetCategory(part)]++;
                }
            }
            finally
            {
                Rand.PopState();
            }

            Log.Message("[UET HIT] === Original 10000 selections ===");
            Log.Message("[UET HIT] Pawn: " + pawn.LabelShort);

            foreach (var pair in counts)
                Log.Message("[UET HIT] " + pair.Key + ": " +
                    pair.Value + " (" +
                    (100f * pair.Value / SampleCount).ToString("F2") + "%)");

            Log.Message("[UET HIT] Null parts: " + nullParts);
        }

        public static void RunSequenceTest(Pawn pawn)
        {
            if (!ValidPawn(pawn))
            {
                Log.Warning("[UET HIT A] Invalid pawn.");
                return;
            }

            var stats = new ArmorTraitSequenceStats();
            DamageDef damage = DamageDefOf.Cut;

            Rand.PushState();
            try
            {
                for (int i = 0; i < SequenceSampleCount; i++)
                {
                    // Diagnostic model of the default 2-4 range.
                    int length = Rand.RangeInclusive(2, 4);
                    var parts = new List<BodyPartRecord>(length);

                    for (int j = 0; j < length; j++)
                    {
                        parts.Add(
                            pawn.health.hediffSet.GetRandomNotMissingPart(
                                damage));
                    }

                    stats.AddSequence(parts);
                }
            }
            finally
            {
                Rand.PopState();
            }

            Log.Message(
                "[UET HIT A] Pawn: " + pawn.LabelShort +
                " | Race: " + pawn.def.defName +
                " | Damage: " + damage.defName);

            Log.Message(stats.BuildReport("SIMULATION A"));
        }

        private static bool ValidPawn(Pawn pawn)
        {
            return pawn != null &&
                   !pawn.Dead &&
                   pawn.RaceProps != null &&
                   pawn.RaceProps.Humanlike &&
                   pawn.health != null &&
                   pawn.health.hediffSet != null;
        }

        private static Dictionary<ArmorTraitCategory, int> NewCounts()
        {
            var result = new Dictionary<ArmorTraitCategory, int>();

            foreach (ArmorTraitCategory category in
                     Enum.GetValues(typeof(ArmorTraitCategory)))
                result[category] = 0;

            return result;
        }
    }


    public sealed class ArmorTraitSequenceStats
    {
        private readonly Dictionary<ArmorTraitCategory, int[]> counts =
            new Dictionary<ArmorTraitCategory, int[]>();

        private readonly Dictionary<int, int> lengths =
            new Dictionary<int, int>();

        // Full anatomical sequences and their frequencies.
        private readonly Dictionary<string, int> sequencesByCategory =
            new Dictionary<string, int>();

        private int sequences;
        private int totalHits;
        private int nullHits;

        public int SequenceCount
        {
            get { return sequences; }
        }

        public ArmorTraitSequenceStats()
        {
            foreach (ArmorTraitCategory category in
                     Enum.GetValues(typeof(ArmorTraitCategory)))
            {
                counts[category] = new int[4];
            }
        }

        public void AddSequence(IList<BodyPartRecord> parts)
        {
            if (parts == null)
                return;

            sequences++;

            int length = parts.Count;

            if (!lengths.ContainsKey(length))
                lengths[length] = 0;

            lengths[length]++;

            var sequence = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                if (i > 0)
                    sequence.Append(" > ");

                BodyPartRecord part = parts[i];

                if (part == null)
                {
                    nullHits++;
                    sequence.Append("None");
                    continue;
                }

                totalHits++;

                ArmorTraitCategory category =
                    ArmorTraitAnatomy.GetCategory(part);

                sequence.Append(category.ToString());

                if (i < 4)
                    counts[category][i]++;
            }

            string key = sequence.ToString();

            if (!sequencesByCategory.ContainsKey(key))
                sequencesByCategory[key] = 0;

            sequencesByCategory[key]++;
        }

        public string BuildReport(string title)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[UET HIT] === " + title + " ===");
            sb.AppendLine("Sequences: " + sequences);
            sb.AppendLine("Valid hits: " + totalHits);
            sb.AppendLine("Null hits: " + nullHits);

            sb.AppendLine();
            sb.AppendLine("=== SEQUENCE LENGTHS ===");

            var sortedLengths = new List<int>(lengths.Keys);
            sortedLengths.Sort();

            foreach (int length in sortedLengths)
            {
                sb.AppendLine(
                    "Length " + length + ": " + lengths[length]);
            }

            sb.AppendLine();
            sb.AppendLine("=== ANATOMICAL POSITIONS ===");
            sb.AppendLine("Category | P1 | P2 | P3 | P4");

            foreach (ArmorTraitCategory category in
                     Enum.GetValues(typeof(ArmorTraitCategory)))
            {
                int[] c = counts[category];

                sb.AppendLine(
                    category + " | " +
                    c[0] + " | " +
                    c[1] + " | " +
                    c[2] + " | " +
                    c[3]);
            }

            sb.AppendLine();
            sb.AppendLine("=== COMPLETE ANATOMICAL SEQUENCES ===");

            var sortedSequences =
                new List<KeyValuePair<string, int>>(
                    sequencesByCategory);

            // Most frequent sequences first.
            sortedSequences.Sort((a, b) =>
            {
                int comparison = b.Value.CompareTo(a.Value);

                if (comparison != 0)
                    return comparison;

                return string.CompareOrdinal(a.Key, b.Key);
            });

            foreach (var entry in sortedSequences)
            {
                sb.AppendLine(
                    entry.Key + " : " + entry.Value);
            }

            sb.AppendLine();
            sb.AppendLine("Unique sequences: " +
                sequencesByCategory.Count);

            sb.AppendLine("[UET HIT] === END ===");

            return sb.ToString();
        }
    }
}

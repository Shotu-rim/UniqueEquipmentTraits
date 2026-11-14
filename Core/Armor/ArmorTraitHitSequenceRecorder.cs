using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitHitSequenceRecorder
    {
        private sealed class HitContext
        {
            public Pawn Pawn;
            public readonly List<BodyPartRecord> Parts = new List<BodyPartRecord>();
            public readonly List<BodyPartRecord> InjuredParts = new List<BodyPartRecord>();
            public readonly Dictionary<string, int> DamageTypes = new Dictionary<string, int>();
            public int DamageInfos;
            public int DamagePartCalls;
            public int ArmorChecks;
            public int Injuries;
        }

        [ThreadStatic] private static Stack<HitContext> contexts;
        private static ArmorTraitSequenceStats stats = new ArmorTraitSequenceStats();
        private static bool enabled;
        private static int attacks;
        private static int totalDamageInfos;
        private static int totalPartCalls;
        private static int totalArmorChecks;
        private static int totalInjuries;
        private static readonly Dictionary<int, int> damageInfoHistogram = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> partCallHistogram = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> armorHistogram = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> injuryHistogram = new Dictionary<int, int>();
        private static readonly Dictionary<string, int> damageTypeTotals = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> injurySequences = new Dictionary<string, int>();

        public static int SequenceCount { get { return stats.SequenceCount; } }
        public static bool IsRecording { get { return enabled; } }

        public static void Start()
        {
            if (enabled) return;
            enabled = true;
            Log.Message("[UET HIT B] Recording enabled.");
        }

        public static void Stop()
        {
            enabled = false;
            Log.Message("[UET HIT B] Recording stopped. Attacks: " + attacks);
        }

        public static void Begin(Pawn pawn)
        {
            if (!enabled) return;
            if (contexts == null) contexts = new Stack<HitContext>();
            contexts.Push(new HitContext { Pawn = pawn });
        }

        private static HitContext Active(Pawn pawn)
        {
            if (!enabled || pawn == null || contexts == null || contexts.Count == 0) return null;
            HitContext ctx = contexts.Peek();
            return ctx.Pawn == pawn ? ctx : null;
        }

        public static void Record(Pawn pawn, BodyPartRecord part)
        {
            HitContext ctx = Active(pawn);
            if (ctx != null) ctx.Parts.Add(part);
        }

        public static void RecordDamageInfo(Pawn pawn, DamageInfo dinfo)
        {
            HitContext ctx = Active(pawn);
            if (ctx == null) return;
            ctx.DamageInfos++;
            string name = dinfo.Def == null ? "Unknown" : dinfo.Def.defName;
            Increment(ctx.DamageTypes, name);
        }

        public static void RecordPartCall(Pawn pawn)
        {
            HitContext ctx = Active(pawn);
            if (ctx != null) ctx.DamagePartCalls++;
        }

        public static void RecordArmorCheck(Pawn pawn)
        {
            HitContext ctx = Active(pawn);
            if (ctx != null) ctx.ArmorChecks++;
        }

        public static void RecordInjury(Pawn pawn, Hediff_Injury injury)
        {
            HitContext ctx = Active(pawn);
            if (ctx == null || injury == null) return;
            ctx.Injuries++;
            ctx.InjuredParts.Add(injury.Part);
        }

        public static void End()
        {
            if (contexts == null || contexts.Count == 0) return;
            HitContext ctx = contexts.Pop();
            if (!enabled || ctx.Pawn == null) return;
            attacks++;
            totalDamageInfos += ctx.DamageInfos;
            totalPartCalls += ctx.DamagePartCalls;
            totalArmorChecks += ctx.ArmorChecks;
            totalInjuries += ctx.Injuries;
            Increment(damageInfoHistogram, ctx.DamageInfos);
            Increment(partCallHistogram, ctx.DamagePartCalls);
            Increment(armorHistogram, ctx.ArmorChecks);
            Increment(injuryHistogram, ctx.Injuries);
            foreach (KeyValuePair<string, int> kv in ctx.DamageTypes)
                Add(damageTypeTotals, kv.Key, kv.Value);
            if (ctx.Parts.Count > 0) stats.AddSequence(ctx.Parts);
            if (ctx.InjuredParts.Count > 0)
            {
                StringBuilder key = new StringBuilder();
                foreach (BodyPartRecord part in ctx.InjuredParts)
                {
                    if (key.Length > 0) key.Append(" -> ");
                    key.Append(part == null ? "NULL" : part.Label);
                }
                Increment(injurySequences, key.ToString());
            }
            if (attacks % 100 == 0)
                Log.Message("[UET HIT B] Recorded melee attacks: " + attacks);
        }

        private static void Increment<TKey>(Dictionary<TKey, int> dict, TKey key)
        {
            int current;
            dict.TryGetValue(key, out current);
            dict[key] = current + 1;
        }

        private static void Add<TKey>(Dictionary<TKey, int> dict, TKey key, int value)
        {
            int current;
            dict.TryGetValue(key, out current);
            dict[key] = current + value;
        }

        private static void AppendHistogram(StringBuilder sb, string heading, Dictionary<int, int> hist)
        {
            sb.AppendLine(heading);
            List<int> keys = new List<int>(hist.Keys);
            keys.Sort();
            foreach (int key in keys) sb.AppendLine("  " + key + ": " + hist[key]);
        }

        private static void AppendNamed(StringBuilder sb, string heading, Dictionary<string, int> values)
        {
            sb.AppendLine(heading);
            List<string> keys = new List<string>(values.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (string key in keys) sb.AppendLine("  " + key + ": " + values[key]);
        }

        public static string BuildReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[UET HIT B] === REAL MELEE COMBAT B / EXTENDED ===");
            sb.AppendLine("Melee attacks: " + attacks);
            sb.AppendLine("DamageInfo processed: " + totalDamageInfos);
            sb.AppendLine("ApplyDamageToPart calls: " + totalPartCalls);
            sb.AppendLine("ArmorUtility checks: " + totalArmorChecks);
            sb.AppendLine("Injury finalizations: " + totalInjuries);
            sb.AppendLine("Note: injury finalizations are attempts to add an injury, not necessarily distinct lasting wounds.");
            sb.AppendLine();
            AppendHistogram(sb, "=== DAMAGEINFO PER ATTACK ===", damageInfoHistogram);
            AppendHistogram(sb, "=== DAMAGE-PART CALLS PER ATTACK ===", partCallHistogram);
            AppendHistogram(sb, "=== ARMOR CHECKS PER ATTACK ===", armorHistogram);
            AppendHistogram(sb, "=== INJURY FINALIZATIONS PER ATTACK ===", injuryHistogram);
            sb.AppendLine();
            AppendNamed(sb, "=== DAMAGE TYPES (PER DAMAGEINFO) ===", damageTypeTotals);
            sb.AppendLine();
            AppendNamed(sb, "=== INJURY PART SEQUENCES (EXACT LABELS) ===", injurySequences);
            sb.AppendLine();
            sb.AppendLine(stats.BuildReport("REAL COMBAT B / INITIAL HIT PARTS"));
            return sb.ToString();
        }

        public static void PrintReport() { Log.Message(BuildReport()); }

        public static void Reset()
        {
            stats = new ArmorTraitSequenceStats();
            attacks = totalDamageInfos = totalPartCalls = totalArmorChecks = totalInjuries = 0;
            damageInfoHistogram.Clear();
            partCallHistogram.Clear();
            armorHistogram.Clear();
            injuryHistogram.Clear();
            damageTypeTotals.Clear();
            injurySequences.Clear();
            if (contexts != null) contexts.Clear();
            Log.Message("[UET HIT B] Statistics reset. Recording " +
                (enabled ? "continues." : "is disabled."));
        }
    }
}

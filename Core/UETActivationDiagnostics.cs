
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    internal static class UETActivationDiagnostics
    {
        private sealed class Counters
        {
            public int Attacks;
            public int DamagingAttacks;
            public int Rolls;
            public int SuccessfulRolls;
            public int DispatchedEffects;
        }

        private static readonly Dictionary<QualityCategory, Counters>
            stats = new Dictionary<QualityCategory, Counters>();

        private static Counters Get(QualityCategory quality)
        {
            if (!stats.TryGetValue(quality, out Counters counters))
            {
                counters = new Counters();
                stats.Add(quality, counters);
            }

            return counters;
        }

        internal static void RecordAttack(
            QualityCategory quality,
            bool damageDealt)
        {
            Counters counters = Get(quality);

            counters.Attacks++;

            if (damageDealt)
                counters.DamagingAttacks++;
        }

        internal static void RecordRoll(
            QualityCategory quality,
            bool success)
        {
            Counters counters = Get(quality);

            counters.Rolls++;

            if (success)
                counters.SuccessfulRolls++;
        }

        internal static void RecordDispatch(
            QualityCategory quality)
        {
            Get(quality).DispatchedEffects++;
        }

        internal static void Reset()
        {
            stats.Clear();
            Log.Message("[UET Diagnostics] Counters reset.");
        }

        internal static void PrintReport()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[UET Diagnostics] Activation report");

            if (stats.Count == 0)
            {
                sb.AppendLine("No attacks recorded.");
                Log.Message(sb.ToString());
                return;
            }

            foreach (QualityCategory quality
                in System.Enum.GetValues(typeof(QualityCategory)))
            {
                if (!stats.TryGetValue(quality, out Counters c))
                    continue;

                float expectedChance =
                    WeaponTraitQuality.GetActivationChance(quality) * 100f;

                float actualChance = c.Rolls > 0
                    ? 100f * c.SuccessfulRolls / c.Rolls
                    : 0f;

                float effectiveChance = c.Attacks > 0
                    ? 100f * c.SuccessfulRolls / c.Attacks
                    : 0f;

                sb.AppendLine();
                sb.AppendLine($"Quality: {quality}");
                sb.AppendLine($"Attacks: {c.Attacks}");
                sb.AppendLine($"Damaging attacks: {c.DamagingAttacks}");
                sb.AppendLine($"Activation rolls: {c.Rolls}");
                sb.AppendLine($"Successful rolls: {c.SuccessfulRolls}");
                sb.AppendLine($"Dispatched effects: {c.DispatchedEffects}");
                sb.AppendLine($"Expected roll chance: {expectedChance:F2}%");
                sb.AppendLine($"Actual roll chance: {actualChance:F2}%");
                sb.AppendLine($"Successful rolls / attacks: {effectiveChance:F2}%");
            }

            Log.Message(sb.ToString());
        }
    }
}


using System.Collections.Generic;
using System.Text;
using RimWorld;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class BloodseekerEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null)
                return;

            BloodseekerHealingReport report =
                BloodseekerDiagnostics.Enabled
                    ? new BloodseekerHealingReport()
                    : null;

            float healed = BloodseekerUtility.TryHeal(
                context.Attacker,
                context.Victim,
                context.Equipment,
                context.BleedingBeforeAttack,
                report);

            if (BloodseekerDiagnostics.Enabled)
            {
                context.Report(BuildDiagnosticReport(context, report));
                return;
            }

            // Normalna rozgrywka: dotychczasowy komunikat.
            if (healed > 0f)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} drew strength " +
                    $"from the attack, recovering {healed:F2} HP.");
            }
        }

        private static string BuildDiagnosticReport(
            UETEffectContext context,
            BloodseekerHealingReport report)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("[BLOODSEEKER DIAGNOSTICS]");

            sb.AppendLine();
            sb.AppendLine("ATTACK");
            sb.AppendLine(
                $"Actual Damage: {context.DamageAmount:F2} HP");

            if (context.PartDamage.Count > 0)
            {
                sb.AppendLine("Injury damage by body part:");

                foreach (KeyValuePair<BodyPartRecord, float> entry
                    in context.PartDamage)
                {
                    sb.AppendLine(
                        $"  {PartName(entry.Key)}: " +
                        $"{entry.Value:F2} HP");
                }
            }
            else
            {
                sb.AppendLine(
                    "Injury damage by body part: none recorded");
            }

            sb.AppendLine();
            sb.AppendLine("BLEEDING");
            sb.AppendLine(
                $"Before attack: " +
                $"{context.BleedingBeforeAttack * 100f:F1}%");

            sb.AppendLine(
                $"After attack: " +
                $"{report.BleedingAfterAttack * 100f:F1}%");

            sb.AppendLine(
                $"Used: {report.BleedingUsed * 100f:F1}%");

            sb.AppendLine(
                $"Healing factor: {report.BleedingFactor:F3}");

            sb.AppendLine();
            sb.AppendLine("ATTACKER INJURIES");

            if (report.InjuriesByPart.Count > 0)
            {
                foreach (KeyValuePair<
                    BodyPartRecord,
                    BloodseekerHealingReport.PartInjuryInfo> entry
                    in report.InjuriesByPart)
                {
                    sb.AppendLine(
                        $"  {PartName(entry.Key)}: " +
                        $"{entry.Value.MissingHP:F2} HP " +
                        $"({entry.Value.WoundCount} wounds)");
                }
            }
            else
            {
                sb.AppendLine("  No healable injuries");
            }

            sb.AppendLine(
                $"Total: {report.MissingHP:F2} HP " +
                $"({report.InjuryCount} wounds)");

            sb.AppendLine();
            sb.AppendLine("HEALING");

            sb.AppendLine(
                $"EffectStrength: {report.EffectStrength:F3}");

            sb.AppendLine(
                $"Initial pool: {report.InitialHealingPool:F2} HP");

            sb.AppendLine(
                $"Part limit: {report.PartLimitFactor * 100f:F1}%");

            if (report.HealedByPart.Count > 0)
            {
                sb.AppendLine("Healing by body part:");

                foreach (KeyValuePair<BodyPartRecord, float> entry
                    in report.HealedByPart)
                {
                    sb.AppendLine(
                        $"  {PartName(entry.Key)}: " +
                        $"{entry.Value:F2} HP");
                }
            }
            else
            {
                sb.AppendLine("Healing by body part: none");
            }

            sb.AppendLine(
                $"Total healed: {report.TotalHealed:F2} HP");

            sb.AppendLine(
                $"Unused pool: {report.UnusedHealingPool:F2} HP");

            sb.AppendLine();
            sb.AppendLine("RESULT");

            if (report.HealingSucceeded)
            {
                sb.Append("Healing successful");
            }
            else
            {
                sb.Append(
                    "No healing: " +
                    (report.SkipReason ?? "Unknown reason"));
            }

            return sb.ToString().TrimEnd();
        }

        private static string PartName(BodyPartRecord part)
        {
            if (part == null)
                return "Unknown part";

            return part.Label;
        }
    }
}

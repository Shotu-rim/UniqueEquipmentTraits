
using System.Collections.Generic;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    public class NaniteDisruptionEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.EffectStrength <= 0f)
                return;

            NaniteDisruptionResult result =
                NaniteDisruptionUtility.TryDisrupt(
                    context.Victim,
                    context.EffectStrength);

            if (result == null || result.Parts.Count == 0)
                return;

            string attacker =
                context.Attacker?.LabelShort ?? "Unknown attacker";

            string victim = context.Victim.LabelShort;

            List<string> details = new List<string>();

            float totalApplied = 0f;

            foreach (NaniteDisruptionPartResult part
                in result.Parts)
            {
                totalApplied += part.AppliedPercent;

                details.Add(
                    $"{part.PartName}: " +
                    $"+{part.AppliedPercent:F1} pp " +
                    $"(total: {part.TotalPercent:F1}%)");
            }

            string targetType = result.IsMechanoid
                ? "mechanical components"
                : "implants";

            context.Report(
                $"{attacker} released disruptive nanites " +
                $"into {victim}, degrading " +
                $"{result.Parts.Count} {targetType} " +
                $"by a total of {totalApplied:F1} " +
                $"percentage points: " +
                string.Join("; ", details) + ".");
        }
    }
}

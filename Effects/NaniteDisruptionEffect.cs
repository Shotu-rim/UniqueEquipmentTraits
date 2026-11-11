
using System.Collections.Generic;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    public class NaniteDisruptionEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null)
                return;

            NaniteDisruptionResult result =
                NaniteDisruptionUtility.TryDisrupt(
                    context.Victim,
                    context.EffectStrength);

            if (result == null || result.Parts.Count == 0)
                return;

            string attacker = context.Attacker.LabelShort;
            string victim = context.Victim.LabelShort;

            if (result.IsMechanoid)
            {
                ReportMechanoid(
                    context,
                    result,
                    attacker,
                    victim);
            }
            else
            {
                ReportImplant(
                    context,
                    result,
                    attacker,
                    victim);
            }
        }

        private static void ReportImplant(
            UETEffectContext context,
            NaniteDisruptionResult result,
            string attacker,
            string victim)
        {
            NaniteDisruptionPartResult part =
                result.Parts[0];

            context.Report(
                $"{attacker} disrupted {victim}'s nanite systems, " +
                $"degrading {part.PartName} by " +
                $"{part.AppliedPercent:F1} percentage points " +
                $"(total degradation: {part.TotalPercent:F1}%).");
        }

        private static void ReportMechanoid(
            UETEffectContext context,
            NaniteDisruptionResult result,
            string attacker,
            string victim)
        {
            List<string> details = new List<string>();

            foreach (NaniteDisruptionPartResult part
                in result.Parts)
            {
                details.Add(
                    $"{part.PartName}: " +
                    $"+{part.AppliedPercent:F1} percentage points " +
                    $"(total: {part.TotalPercent:F1}%)");
            }

            context.Report(
                $"{attacker} disrupted {victim}'s mechanical systems, " +
                $"degrading {result.Parts.Count} " +
                $"{(result.Parts.Count == 1 ? "body part" : "body parts")}: " +
                string.Join("; ", details) + ".");
        }
    }
}

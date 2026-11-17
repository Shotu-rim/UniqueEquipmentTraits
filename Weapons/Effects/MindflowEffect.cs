
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class MindflowEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Attacker.Dead ||
                context.DamageAmount <= 0f)
                return;

            Pawn attacker = context.Attacker;

            Pawn_PsychicEntropyTracker tracker =
                attacker.psychicEntropy;

            if (tracker == null)
                return;

            float currentHeat = tracker.EntropyValue;
            float maxHeat = tracker.MaxEntropy;

            if (currentHeat <= 0f || maxHeat <= 0f)
                return;

            float minPercent;
            float maxPercent;

            switch (context.Quality)
            {
                case QualityCategory.Masterwork:
                    minPercent = 0.05f;
                    maxPercent = 0.15f;
                    break;

                case QualityCategory.Legendary:
                    minPercent = 0.10f;
                    maxPercent = 0.20f;
                    break;

                default:
                    return;
            }

            float reduction =
                maxHeat * Rand.Range(minPercent, maxPercent);

            reduction = Mathf.Min(reduction, currentHeat);

            if (reduction <= 0f)
                return;

            tracker.TryAddEntropy(
                -reduction,
                scale: false);

            float actualReduction =
                currentHeat - tracker.EntropyValue;

            if (actualReduction > 0f)
            {
                context.Report(
                    $"{attacker.LabelShort} activated Mindflow, " +
                    $"reducing Neural Heat by " +
                    $"{actualReduction:F1} " +
                    $"({currentHeat:F1} → " +
                    $"{tracker.EntropyValue:F1}).");
            }
        }
    }
}

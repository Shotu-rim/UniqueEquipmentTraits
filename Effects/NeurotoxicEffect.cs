
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    public class NeurotoxicEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.EffectStrength <= 0f)
            {
                return;
            }

            int affectedCapacities;
            float appliedWeakening;

            NeurotoxicUtility.ApplyNeurotoxic(
                context.Victim,
                context.EffectStrength,
                out affectedCapacities,
                out appliedWeakening);

            // No toxic if impossible
            if (affectedCapacities <= 0 ||
                appliedWeakening <= 0f)
            {
                return;
            }

            string attacker =
                context.Attacker?.LabelShort ?? "Unknown attacker";

            string victim = context.Victim.LabelShort;

            float appliedPercent =
                appliedWeakening * 100f;

            context.Report(
                $"{attacker} poisoned {victim} with a neurotoxin, " +
                $"impairing {affectedCapacities} " +
                $"{(affectedCapacities == 1 ? "capacity" : "capacities")} " +
                $"by a total of {appliedPercent:F1} percentage points.");
        }
    }
}

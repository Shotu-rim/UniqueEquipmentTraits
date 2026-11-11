
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    public class NeurotoxicEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            if (context.EffectStrength <= 0f)
                return;

            NeurotoxicUtility.ApplyNeurotoxic(
                context.Victim,
                context.EffectStrength);

            string attacker =
                context.Attacker?.LabelShort ?? "Unknown attacker";

            string victim = context.Victim.LabelShort;

            context.Report(
                $"{attacker} poisoned {victim} with a neurotoxin, " +
                "impairing their bodily functions.");
        }
    }
}

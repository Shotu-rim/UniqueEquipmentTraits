
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    public class BloodseekerEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null)
                return;

            float healed = BloodseekerUtility.TryHeal(
                context.Attacker,
                context.Victim,
                context.Equipment,
                context.BleedingBeforeAttack);

            if (healed > 0f)
            {
                context.Report(
                    $"{context.Attacker.LabelShort}'s Bloodseeker " +
                    $"feasted on {context.Victim.LabelShort}'s wounds, " +
                    $"restoring {healed:F2} HP.");
            }
        }
    }
}


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

            // Raportujemy wyłącznie rzeczywiste leczenie.
            if (healed > 0f)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} drew strength from the attack, " +
                    $"recovering {healed:F2} HP.");
            }
        }
    }
}

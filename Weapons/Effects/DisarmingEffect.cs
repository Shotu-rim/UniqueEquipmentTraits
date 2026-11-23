
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class DisarmingEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            ThingWithComps dropped;

            if (!UETDisarmUtility.TryDisarm(
                context.Victim,
                out dropped))
            {
                return;
            }

            context.Report(
                context.Attacker.LabelShort +
                " disarmed " +
                context.Victim.LabelShort +
                ", knocking " +
                dropped.LabelShort +
                " to the ground.");
        }
    }
}

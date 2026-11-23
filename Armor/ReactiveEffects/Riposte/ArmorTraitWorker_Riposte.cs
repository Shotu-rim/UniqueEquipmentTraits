
using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_Riposte : ArmorTraitWorker
    {
        public override bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return context != null &&
                   context.Attacker != null &&
                   context.Defender != null &&
                   context.Apparel != null &&
                   context.Trait != null &&
                   !context.Attacker.Dead &&
                   context.Attacker.Spawned;
        }

        public override void OnActivated(
            ArmorTraitActivationContext context)
        {
            if (!CanActivate(context))
                return;

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return;

            float chance;

            switch (quality)
            {
                case QualityCategory.Masterwork:
                    chance = 0.25f;
                    break;

                case QualityCategory.Legendary:
                    chance = 0.35f;
                    break;

                default:
                    return;
            }

            if (!Rand.Chance(chance))
                return;

            ThingWithComps dropped;

            if (!UETDisarmUtility.TryDisarm(
                context.Attacker,
                out dropped))
            {
                return;
            }

            string description =
                context.Defender.LabelShort +
                "'s " +
                context.Apparel.LabelShort +
                " disarmed " +
                context.Attacker.LabelShort +
                ", knocking " +
                dropped.LabelShort +
                " to the ground.";

            UETCombatLog.Record(
                context.Trait,
                context.Defender,
                description,
                context.Attacker);
        }
    }
}


using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_Shock : ArmorTraitWorker
    {
        public override bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return context != null &&
                   context.Attacker != null &&
                   context.Defender != null &&
                   context.Apparel != null &&
                   context.Trait != null &&
                   UETShockUtility.CanShock(context.Attacker);
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
            int stunTicks;

            switch (quality)
            {
                case QualityCategory.Masterwork:
                    chance = 0.45f;
                    stunTicks = 120;
                    break;

                case QualityCategory.Legendary:
                    chance = 0.525f;
                    stunTicks = 180;
                    break;

                default:
                    return;
            }

            if (!Rand.Chance(chance))
                return;

            if (!UETShockUtility.ApplyShock(
                context.Attacker,
                context.Defender,
                stunTicks))
            {
                return;
            }


            string description =
                context.Defender.LabelShort +
                "'s " +
                context.Apparel.LabelShort +
                " releases a powerful electrical discharge, " +
                "stunning " +
                context.Attacker.LabelShort +
                " for " +
                (stunTicks / 60f).ToString("0.#") +
                " seconds.";

            UETCombatLog.Record(
                context.Trait,
                context.Defender,
                description,
                context.Attacker);

        }
    }
}

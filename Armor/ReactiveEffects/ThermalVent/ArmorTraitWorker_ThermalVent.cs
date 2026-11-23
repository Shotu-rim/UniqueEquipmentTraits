
using RimWorld;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_ThermalVent
        : ArmorTraitWorker
    {
        public override bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return context != null &&
                   context.Defender != null &&
                   context.Apparel != null &&
                   context.InjurySeverity > 0f &&
                   UETThermalVentUtility.HasTargets(
                       context.Defender) &&
                   !UETThermalVentGuard.IsActive;
        }

        public override float GetActivationChance(
            ArmorTraitActivationContext context)
        {
            if (context == null ||
                context.Apparel == null)
            {
                return 0f;
            }

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return 0f;

            return WeaponTraitQuality.GetEffectStrength(
                quality);
        }

        public override int GetCooldownTicks(
            ArmorTraitActivationContext context)
        {
            if (context == null ||
                context.Apparel == null)
            {
                return 900;
            }

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return 900;

            return quality == QualityCategory.Legendary
                ? 600
                : 900;
        }

        public override bool TryActivate(
            ArmorTraitActivationContext context)
        {
            if (!CanActivate(context))
                return false;

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return false;

            int affectedTargets =
                UETThermalVentUtility.Emit(
                    context.Defender,
                    quality);

            if (affectedTargets <= 0)
                return false;

            string description =
                context.Defender.LabelShort + "'s " +
                context.Apparel.LabelShort +
                " releases a violent burst of superheated air, " +
                "scorching " +
                affectedTargets +
                (affectedTargets == 1
                    ? " nearby creature."
                    : " nearby creatures.");

            UETCombatLog.Record(
                context.Trait,
                context.Defender,
                description,
                context.Attacker);

            return true;
        }
    }
}

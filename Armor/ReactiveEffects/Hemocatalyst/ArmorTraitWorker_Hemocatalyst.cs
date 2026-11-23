
using RimWorld;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_Hemocatalyst
        : ArmorTraitWorker
    {
        public override bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return context != null &&
                   context.Defender != null &&
                   context.InjurySeverity > 0f &&
                   UETHemocatalystUtility.CanRestoreFocus(
                       context.Defender);
        }

        public override float GetActivationChance(
            ArmorTraitActivationContext context)
        {
            if (context == null || context.Apparel == null)
                return 0f;

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return 0f;

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            return strength * 2f;
        }

        public override bool TryActivate(
            ArmorTraitActivationContext context)
        {
            if (!CanActivate(context))
                return false;

            QualityCategory quality;

            if (!context.Apparel.TryGetQuality(out quality))
                return false;

            float gain =
                UETHemocatalystUtility.CalculateFocusGain(
                    quality,
                    context.InjurySeverity);

            if (gain <= 0f)
                return false;

            float restored =
                UETHemocatalystUtility.RestoreFocus(
                    context.Defender,
                    gain);

            if (restored <= 0f)
                return false;

            UETCombatLog.Record(
                context.Trait,
                context.Defender,
                "hemocatalytic conduits convert the trauma of incoming injuries " +
                "into psychic energy, restoring " +
                (restored * 100f).ToString("0.#") +
                "% psyfocus",
                context.Attacker);

            return true;
        }
    }
}

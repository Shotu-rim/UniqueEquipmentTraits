
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using UnityEngine;
using CompWeaponTrait = UniqueEquipmentTraits.Traits.CompWeaponTrait;

namespace UniqueEquipmentTraits.Effects
{

    public class Hediff_FlurryActive : HediffWithComps
    {
        public override Color LabelColor => Color.green;

        public override string TipStringExtra
        {
            get
            {
                string original = base.TipStringExtra;

                Pawn owner = pawn;
                ThingWithComps weapon = owner?.equipment?.Primary;

                CompWeaponTrait comp =
                    weapon?.TryGetComp<CompWeaponTrait>();

                if (comp == null ||
                    !comp.FlurryActiveFor(owner))
                    return original;

                CompQuality quality =
                    weapon.TryGetComp<CompQuality>();

                int maximum = quality == null
                    ? 0
                    : FlurryEffect.InitialCharges(quality.Quality);

                int remaining = comp.FlurryCharges;

                float speedBonus = quality == null
                    ? 0f
                    : FlurryEffect.SpeedBonus(quality.Quality);

                float damageMultiplier = quality == null
                    ? 1f
                    : FlurryEffect.DamageMultiplier(quality.Quality);

                string details =
                    "Melee attack speed: +" +
                    (speedBonus * 100f).ToString("0") + "%\n" +
                    "Melee damage: -" +
                    ((1f - damageMultiplier) * 100f).ToString("0") + "%\n" +
                    "Charges remaining: " +
                    remaining + " / " + maximum;

                return string.IsNullOrEmpty(original)
                    ? details
                    : original + "\n" + details;
            }
        }
    }


    internal static class FlurryEffect
    {
        private const string FlurryTraitDefName = "UET_Flurry";
        private const string FlurryHediffDefName = "UET_FlurryActive";

        // Check weapon for Flurry
        internal static bool HasFlurry(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;

            CompUniqueWeapon unique =
                weapon.TryGetComp<CompUniqueWeapon>();

            if (unique == null ||
                unique.TraitsListForReading == null)
                return false;

            foreach (RimWorld.WeaponTraitDef trait in unique.TraitsListForReading)
            {
                if (trait != null &&
                    trait.defName == FlurryTraitDefName)
                    return true;
            }

            return false;
        }

        internal static float ActivationChance(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.30f;

                case QualityCategory.Legendary:
                    return 0.35f;

                default:
                    return 0f;
            }
        }

        // Charges after activation
        internal static int InitialCharges(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 8;

                case QualityCategory.Legendary:
                    return 10;

                default:
                    return 0;
            }
        }

        // Bonuses
        internal static float SpeedBonus(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.70f;

                case QualityCategory.Legendary:
                    return 0.90f;

                default:
                    return 0f;
            }
        }

        // Count damage, reduced by armor on hit
        internal static float DamageMultiplier(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.70f;

                case QualityCategory.Legendary:
                    return 0.65f;

                default:
                    return 1f;
            }
        }

        // Check for the same weapon
        internal static bool IsValidOwner(
            ThingWithComps weapon,
            Pawn pawn)
        {
            return weapon != null &&
                   pawn != null &&
                   !pawn.Dead &&
                   pawn.equipment != null &&
                   pawn.equipment.Primary == weapon &&
                   HasFlurry(weapon);
        }

        // Add Hediff to Health
        internal static void SyncHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    FlurryHediffDefName);

            if (def == null)
            {
                Log.WarningOnce(
                    "[UET Flurry] Nie znaleziono HediffDef: " +
                    FlurryHediffDefName,
                    17360421);
                return;
            }

            if (pawn.health.hediffSet.GetFirstHediffOfDef(def) == null)
                pawn.health.AddHediff(def);
        }

        // Remove Hediff at the end
        internal static void RemoveHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    FlurryHediffDefName);

            if (def == null)
                return;

            Hediff hediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(def);

            if (hediff != null)
                pawn.health.RemoveHediff(hediff);
        }


        internal static bool CanUseEffect(
            ThingWithComps weapon,
            Pawn pawn)
        {
            return weapon != null &&
                   pawn != null &&
                   !pawn.Dead &&
                   pawn.equipment != null &&
                   pawn.equipment.Primary == weapon &&
                   (HasFlurry(weapon) ||
                    PrismaticEffect.HasPrismatic(weapon));
        }


    }
}

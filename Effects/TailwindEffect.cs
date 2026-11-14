
using RimWorld;
using Verse;
using UnityEngine;
using CompWeaponTrait = UniqueEquipmentTraits.Traits.CompWeaponTrait;

namespace UniqueEquipmentTraits.Effects
{

    public class Hediff_TailwindActive : HediffWithComps
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
                    !comp.TailwindActiveFor(owner))
                    return original;

                CompQuality quality =
                    weapon.TryGetComp<CompQuality>();

                int maximum = quality == null
                    ? 0
                    : TailwindEffect.InitialCharges(quality.Quality);

                int remaining = comp.TailwindCharges;

                float dodgeBonus = comp.TailwindDodgeBonus();

                string details =
                    "Melee dodge chance: +" +
                    (dodgeBonus * 100f).ToString("0") +
                    " percentage points\n" +
                    "Charges remaining: " +
                    remaining + " / " + maximum;

                return string.IsNullOrEmpty(original)
                    ? details
                    : original + "\n" + details;
            }
        }
    }


    internal static class TailwindEffect
    {
        private const string TraitDefName = "UET_Tailwind";
        private const string HediffDefName = "UET_TailwindActive";

        // Check for Tailwind
        internal static bool HasTailwind(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;

            CompUniqueWeapon unique =
                weapon.TryGetComp<CompUniqueWeapon>();

            if (unique == null ||
                unique.TraitsListForReading == null)
                return false;

            foreach (WeaponTraitDef trait in unique.TraitsListForReading)
            {
                if (trait != null && trait.defName == TraitDefName)
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

        // Dodge bonus for each charge
        internal static float DodgeBonus(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.15f;

                case QualityCategory.Legendary:
                    return 0.20f;

                default:
                    return 0f;
            }
        }

        // Check for weapon carrier
        internal static bool IsValidOwner(
            ThingWithComps weapon,
            Pawn pawn)
        {
            return weapon != null &&
                   pawn != null &&
                   !pawn.Dead &&
                   pawn.equipment != null &&
                   pawn.equipment.Primary == weapon &&
                   HasTailwind(weapon);
        }

        internal static void SyncHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    HediffDefName);

            if (def == null)
            {
                Log.WarningOnce(
                    "[UET Tailwind] Missing HediffDef: " +
                    HediffDefName,
                    17360422);
                return;
            }

            if (pawn.health.hediffSet.GetFirstHediffOfDef(def) == null)
                pawn.health.AddHediff(def);
        }

        internal static void RemoveHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    HediffDefName);

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
                   (HasTailwind(weapon) ||
                    PrismaticEffect.HasPrismatic(weapon));
        }


    }
}

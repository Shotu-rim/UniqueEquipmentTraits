
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitMedicUtility
    {
        public static float GetRange(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 15f;

                case QualityCategory.Legendary:
                    return 20f;

                default:
                    return 0f;
            }
        }

        public static float GetHealingPool(QualityCategory quality)
        {
            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return 0f;

            return 600f * strength - 150f;
        }

        public static bool CanHeal(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (caster == null || target == null ||
                caster.Dead || caster.Downed ||
                !caster.Spawned || caster.Map == null ||
                target.Dead || !target.Spawned ||
                target.Map != caster.Map ||
                state == null || state.IsEffectActive ||
                state.IsOnCooldown || !state.UsesCharges ||
                !state.HasAvailableCharge)
                return false;

            float range = GetRange(quality);

            if (range <= 0f ||
                !caster.Position.InHorDistOf(target.Position, range))
                return false;

            return HasHealableInjuries(target);
        }

        private static bool HasHealableInjuries(Pawn pawn)
        {
            if (pawn.health == null ||
                pawn.health.hediffSet == null)
                return false;

            List<Hediff> hediffs =
                pawn.health.hediffSet.hediffs;

            foreach (Hediff hediff in hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;

                if (injury != null &&
                    injury.Severity > 0f &&
                    !injury.IsPermanent())
                    return true;
            }

            return false;
        }

        public static bool TryHeal(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanHeal(caster, target, quality, state))
                return false;

            float remaining = GetHealingPool(quality);

            if (remaining <= 0f)
                return false;

            List<Hediff_Injury> injuries =
                new List<Hediff_Injury>();

            foreach (Hediff hediff in
                target.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;

                if (injury != null &&
                    injury.Severity > 0f &&
                    !injury.IsPermanent())
                {
                    injuries.Add(injury);
                }
            }

            if (injuries.Count == 0)
                return false;

            // Randomize the order of eligible injuries.
            injuries.Shuffle();

            foreach (Hediff_Injury injury in injuries)
            {
                if (remaining <= 0f)
                    break;

                float healAmount =
                    UnityEngine.Mathf.Min(
                        injury.Severity,
                        remaining);

                if (healAmount <= 0f)
                    continue;

                injury.Heal(healAmount);
                remaining -= healAmount;
            }

            // Consume one charge only after healing.
            if (!state.TryConsumeCharge())
                return false;

            return true;
        }
    }
}

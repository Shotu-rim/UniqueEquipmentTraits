
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using UniqueEquipmentTraits.Effects;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitShreddingUtility
    {
        private const int CooldownTicks = 600;

        public static float GetBleedingMultiplier(
            QualityCategory quality)
        {
            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return 1f;

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            return 10f * strength - 1.25f;
        }

        public static int GetCooldownTicks()
        {
            return CooldownTicks;
        }

        public static bool CanApply(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (caster == null ||
                target == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                target.Dead ||
                !target.Spawned ||
                target.Map != caster.Map ||
                target.health == null ||
                !target.health.CanBleed ||
                state == null ||
                state.IsEffectActive ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
                return false;

            if (GetBleedingMultiplier(quality) <= 1f)
                return false;

            float range = ArmorTraitMedicUtility.GetRange(quality);

            if (!target.Position.InHorDistOf(
                    caster.Position, range))
                return false;

            return HasBleedingInjuries(target);
        }

        private static bool HasBleedingInjuries(Pawn pawn)
        {
            foreach (Hediff hediff in
                pawn.health.hediffSet.hediffs)
            {
                Hediff_Injury injury =
                    hediff as Hediff_Injury;

                if (injury != null &&
                    injury.BleedRate > 0f)
                    return true;
            }

            return false;
        }

        public static bool TryApply(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanApply(caster, target, quality, state))
                return false;

            float multiplier =
                GetBleedingMultiplier(quality);

            List<Hediff_Injury> injuries =
                new List<Hediff_Injury>();

            foreach (Hediff hediff in
                target.health.hediffSet.hediffs)
            {
                Hediff_Injury injury =
                    hediff as Hediff_Injury;

                if (injury != null &&
                    injury.BleedRate > 0f)
                {
                    injuries.Add(injury);
                }
            }

            if (injuries.Count == 0)
                return false;

            HediffDef shreddingDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_ShreddingBleeding");

            if (shreddingDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_ShreddingBleeding");
                return false;
            }

            Hediff_ShreddingBleeding shredding =
                target.health.hediffSet
                    .GetFirstHediffOfDef(shreddingDef)
                    as Hediff_ShreddingBleeding;

            if (shredding == null)
            {
                shredding =
                    HediffMaker.MakeHediff(
                        shreddingDef,
                        target)
                    as Hediff_ShreddingBleeding;

                if (shredding == null)
                {
                    Log.Error(
                        "[UET] Shredding Hediff has an invalid hediffClass.");
                    return false;
                }

                target.health.AddHediff(shredding);
            }

            foreach (Hediff_Injury injury in injuries)
            {
                shredding.ApplyToInjury(
                    injury,
                    multiplier);
            }

            target.health.Notify_HediffChanged(shredding);

            if (!state.TryConsumeCharge())
                return false;

            state.StartCooldown(CooldownTicks);

            return true;
        }
    }
}

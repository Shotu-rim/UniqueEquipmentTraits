
using RimWorld;
using System;
using UniqueEquipmentTraits.Effects;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitOppressionUtility
    {
        private const int CooldownTicks = 600;

        private const float MasterworkRange = 20f;
        private const float LegendaryRange = 25f;

        private const float ReferenceBodyMass = 70f;
        private const float ReferenceEquipmentMass = 15f;

        private const float BodySizeExponent = 0.20f;
        private const float BodyMassExponent = 0.12f;

        public static float GetRange(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return MasterworkRange;

                case QualityCategory.Legendary:
                    return LegendaryRange;

                default:
                    return 0f;
            }
        }

        public static int GetCooldownTicks()
        {
            return CooldownTicks;
        }

        public static int GetDurationTicks(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 18 * 60;

                case QualityCategory.Legendary:
                    return 25 * 60;

                default:
                    return 0;
            }
        }

        public static float GetBaseSlowdown(
            QualityCategory quality)
        {
            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
            {
                return 0f;
            }

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            return 2f * strength;
        }

        public static float GetEquipmentMass(Pawn pawn)
        {
            if (pawn == null)
                return 0f;

            float mass = 0f;

            if (pawn.apparel != null)
            {
                foreach (Apparel apparel in
                    pawn.apparel.WornApparel)
                {
                    mass += Mathf.Max(
                        0f,
                        apparel.GetStatValue(
                            StatDefOf.Mass));
                }
            }

            if (pawn.equipment != null)
            {
                foreach (ThingWithComps equipment in
                    pawn.equipment.AllEquipmentListForReading)
                {
                    mass += Mathf.Max(
                        0f,
                        equipment.GetStatValue(
                            StatDefOf.Mass));
                }
            }

            return mass;
        }

        public static float CalculateSlowdown(
            Pawn target,
            QualityCategory quality)
        {
            if (target == null)
                return 0f;

            float baseSlowdown =
                GetBaseSlowdown(quality);

            if (baseSlowdown <= 0f)
                return 0f;

            float bodySize =
                Mathf.Max(0.25f, target.BodySize);

            float bodyMass =
                Mathf.Max(
                    5f,
                    target.GetStatValue(StatDefOf.Mass));

            float equipmentMass =
                GetEquipmentMass(target);

            double sizeFactor = Math.Pow(
                1.0 / bodySize,
                BodySizeExponent);

            double massFactor = Math.Pow(
                ReferenceBodyMass / bodyMass,
                BodyMassExponent);

            float bodyFactor = Mathf.Clamp(
                (float)(sizeFactor * massFactor),
                0.4f,
                1.4f);

            float equipmentFactor =
                1f - 0.10f *
                (equipmentMass /
                (equipmentMass + ReferenceEquipmentMass));

            return Mathf.Clamp(
                baseSlowdown *
                bodyFactor *
                equipmentFactor,
                0f,
                0.95f);

        }

        public static bool CanActivate(
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
                state == null ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            float range = GetRange(quality);

            if (range <= 0f ||
                GetDurationTicks(quality) <= 0)
            {
                return false;
            }

            return caster.Position.InHorDistOf(
                target.Position,
                range);
        }

        public static bool TryActivate(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanActivate(
                caster,
                target,
                quality,
                state))
            {
                return false;
            }

            HediffDef oppressionDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Oppression");

            if (oppressionDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_Oppression");
                return false;
            }

            float slowdown =
                CalculateSlowdown(target, quality);

            int durationTicks =
                GetDurationTicks(quality);

            if (slowdown <= 0f ||
                durationTicks <= 0)
            {
                return false;
            }

            Hediff_Oppression oppression =
                target.health.hediffSet
                    .GetFirstHediffOfDef(oppressionDef)
                    as Hediff_Oppression;

            bool isNew = oppression == null;

            if (isNew)
            {
                oppression =
                    HediffMaker.MakeHediff(
                        oppressionDef,
                        target)
                    as Hediff_Oppression;

                if (oppression == null)
                {
                    Log.Error(
                        "[UET] UET_Oppression has an invalid hediffClass.");
                    return false;
                }
            }

            if (!state.TryConsumeCharge())
                return false;

            state.StartCooldown(CooldownTicks);

            if (isNew)
            {
                target.health.AddHediff(oppression);
            }

            oppression.ApplyOppression(
                slowdown,
                durationTicks);

            return true;
        }
    }
}

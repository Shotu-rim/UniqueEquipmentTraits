
using System;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class UETStunUtility
    {
        private const float StormboundMultiplier = 2.5f;
        private const float StormboundMaxSeconds = 6f;
        private const float TicksPerSecond = 60f;

        public static int CalculateStormboundTicks(
            QualityCategory quality,
            float bodySize)
        {
            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (strength <= 0f)
                return 0;

            float baseSeconds =
                (strength - 0.05f) * 10f;

            float adjustedBodySize =
                Math.Max(1f, bodySize);

            float bodySizeFactor =
                (float)Math.Pow(adjustedBodySize, 0.25);

            float stunSeconds =
                baseSeconds * StormboundMultiplier /
                bodySizeFactor;

            stunSeconds = Math.Min(
                stunSeconds,
                StormboundMaxSeconds
            );

            return (int)Math.Round(
                stunSeconds * TicksPerSecond
            );
        }

        public static int CalculateConcussiveTicks(
            QualityCategory quality,
            float bodySize)
        {
            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (strength <= 0f)
                return 0;

            float baseSeconds =
                (strength - 0.05f) * 10f;

            float adjustedBodySize =
                Math.Max(1f, bodySize);

            float bodySizeFactor =
                (float)Math.Pow(adjustedBodySize, 0.25);

            float stunSeconds =
                baseSeconds / bodySizeFactor;

            return (int)Math.Round(
                stunSeconds * TicksPerSecond
            );
        }

        public static void ApplyEMPStun(
            Pawn attacker,
            Pawn victim,
            int stunTicks)
        {
            if (attacker == null || victim == null)
                return;

            if (victim.Dead || victim.Downed)
                return;

            if (victim.RaceProps == null ||
                !victim.RaceProps.IsMechanoid)
                return;

            if (stunTicks <= 0)
                return;

            if (victim.stances == null ||
                victim.stances.stunner == null)
                return;

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.EMP,
                stunTicks / 30f,
                0f,
                -1f,
                attacker
            );

            victim.stances.stunner.Notify_DamageApplied(dinfo);
        }

        public static void ApplyNerveStun(
            Pawn attacker,
            Pawn victim,
            int stunTicks)
        {
            if (attacker == null || victim == null)
                return;

            if (victim.Dead || victim.Downed)
                return;

            if (victim.RaceProps == null ||
                !victim.RaceProps.IsFlesh ||
                victim.RaceProps.IsMechanoid)
                return;

            if (stunTicks <= 0)
                return;

            if (victim.stances == null ||
                victim.stances.stunner == null)
                return;

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.NerveStun,
                stunTicks / 30f,
                0f,
                -1f,
                attacker
            );

            victim.stances.stunner.Notify_DamageApplied(dinfo);
        }
    }
}

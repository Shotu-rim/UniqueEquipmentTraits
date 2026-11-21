
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitBerserkUtility
    {
        private const float BaseDurationSeconds = 20f;
        private const float DurationScaling = 200f;
        private const float MaxPsychicSensitivity = 2f;
        private const float TicksPerSecond = 60f;

        private const int MasterworkGoodwillPenalty = -75;
        private const int LegendaryGoodwillPenalty = -60;

        public static int GetDurationTicks(
            QualityCategory quality,
            float psychicSensitivity)
        {
            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (strength <= 0f || psychicSensitivity <= 0f)
                return 0;

            float baseDuration =
                BaseDurationSeconds +
                DurationScaling * (strength - 0.30f);

            float multiplier =
                Mathf.Clamp(
                    psychicSensitivity,
                    0f,
                    MaxPsychicSensitivity);

            return Mathf.Max(
                0,
                Mathf.RoundToInt(
                    baseDuration * multiplier * TicksPerSecond));
        }

        public static int GetGoodwillPenalty(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return MasterworkGoodwillPenalty;

                case QualityCategory.Legendary:
                    return LegendaryGoodwillPenalty;

                default:
                    return 0;
            }
        }

        public static bool CanTarget(
            Pawn caster,
            Pawn target,
            ArmorTraitState state)
        {
            if (caster == null ||
                target == null ||
                state == null ||
                caster.Dead ||
                target.Dead ||
                !caster.Spawned ||
                !target.Spawned ||
                caster.Map != target.Map ||
                target.InMentalState ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            if (target.RaceProps.IsMechanoid)
                return false;

            if (target.GetStatValue(
                StatDefOf.PsychicSensitivity) <= 0f)
            {
                return false;
            }

            return true;
        }

        public static bool TryApply(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanTarget(caster, target, state))
                return false;

            if (state.Charges <= 0)
                return false;

            float sensitivity =
                target.GetStatValue(
                    StatDefOf.PsychicSensitivity);

            int durationTicks =
                GetDurationTicks(quality, sensitivity);

            if (durationTicks <= 0)
                return false;

            Faction targetFaction = target.Faction;
            Faction playerFaction = Faction.OfPlayer;

            bool applyGoodwillPenalty =
                caster.Faction == playerFaction &&
                targetFaction != null &&
                playerFaction != null &&
                targetFaction != playerFaction &&
                !targetFaction.HostileTo(playerFaction);

            MentalStateHandler handler =
                target.mindState.mentalStateHandler;

            bool started = handler.TryStartMentalState(
                MentalStateDefOf.Berserk,
                null,
                forced: true,
                forceWake: true,
                causedByMood: false,
                otherPawn: caster,
                transitionSilently: false,
                causedByDamage: false,
                causedByPsycast: false);

            if (!started)
                return false;

            MentalState mentalState = handler.CurState;

            if (mentalState != null)
            {
                mentalState.forceRecoverAfterTicks =
                    durationTicks;

                mentalState.sourceFaction = caster.Faction;
            }


            state.TryConsumeCharge();

            if (applyGoodwillPenalty)
            {
                int penalty = GetGoodwillPenalty(quality);

                if (penalty != 0)
                {
                    targetFaction.TryAffectGoodwillWith(
                        playerFaction,
                        penalty,
                        canSendMessage: true);
                }
            }

            return true;
        }
    }
}

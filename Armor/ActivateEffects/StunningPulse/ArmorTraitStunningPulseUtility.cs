
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitStunningPulseUtility
    {
        private const float Range = 32f;
        private const int CooldownTicks = 600;

        public static float GetRange()
        {
            return Range;
        }

        public static int GetCooldownTicks()
        {
            return CooldownTicks;
        }

        public static int GetStunTicks(
            QualityCategory quality)
        {
            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return 0;

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            float seconds = 60f * strength - 15f;

            return Mathf.RoundToInt(seconds * 60f);
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
                target.Downed ||
                !target.Spawned ||
                target.Map != caster.Map ||
                target.stances == null ||
                target.stances.stunner == null ||
                state == null ||
                state.IsEffectActive ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            if (GetStunTicks(quality) <= 0)
                return false;

            return caster.Position.InHorDistOf(
                target.Position,
                Range);
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

            DamageDef damageDef =
                DefDatabase<DamageDef>.GetNamedSilentFail(
                    "UET_StunningPulse");

            if (damageDef == null)
            {
                Log.Error(
                    "[UET] Missing DamageDef: UET_StunningPulse");
                return false;
            }

            int stunTicks = GetStunTicks(quality);

            if (stunTicks <= 0)
                return false;

            // Vanilla converts damage amount into stun duration
            // using 30 ticks per damage point.
            float damageAmount = stunTicks / 30f;

            DamageInfo damageInfo = new DamageInfo(
                damageDef,
                damageAmount,
                0f,
                -1f,
                caster);

            // Charge is consumed even when the target
            // has already adapted to Stunning Pulse.
            if (!state.TryConsumeCharge())
                return false;

            state.StartCooldown(CooldownTicks);

            // Use vanilla adaptation and stun handling.
            target.stances.stunner.Notify_DamageApplied(
                damageInfo);

            return true;
        }
    }
}


using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSystemshockUtility
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

        public static float GetRadius(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 2f;

                case QualityCategory.Legendary:
                    return 3f;

                default:
                    return 0f;
            }
        }

        public static int GetCooldownTicks(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 30 * 60;

                case QualityCategory.Legendary:
                    return 25 * 60;

                default:
                    return 0;
            }
        }

        public static int GetGoodwillPenalty(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return -30;

                case QualityCategory.Legendary:
                    return -15;

                default:
                    return 0;
            }
        }

        public static bool CanActivate(
            Pawn pawn,
            IntVec3 target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                !pawn.Spawned ||
                pawn.Map == null ||
                state == null ||
                state.IsOnCooldown ||
                state.IsEffectActive ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            if (!target.IsValid ||
                !target.InBounds(pawn.Map))
            {
                return false;
            }

            float range = GetRange(quality);

            if (range <= 0f ||
                GetRadius(quality) <= 0f)
            {
                return false;
            }

            return (pawn.Position - target)
                .LengthHorizontalSquared <= range * range;
        }

        public static bool TryActivate(
            Pawn pawn,
            IntVec3 target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanActivate(pawn, target, quality, state))
                return false;

            Map map = pawn.Map;
            float radius = GetRadius(quality);
            int cooldownTicks = GetCooldownTicks(quality);

            if (map == null ||
                radius <= 0f ||
                cooldownTicks <= 0)
            {
                return false;
            }

            // Consume one charge only after validation.
            if (!state.TryConsumeCharge())
                return false;

            // Native RimWorld EMP explosion.
            ArmorTraitSystemshockContext.Begin(pawn, quality);

            try
            {
                GenExplosion.DoExplosion(
                            center: target,
                map: map,
                radius: radius,
                damType: DamageDefOf.EMP,
                instigator: pawn,
                damAmount: -1,
                armorPenetration: -1f,
                explosionSound: null,
                weapon: null,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 1,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 1,
                chanceToStartFire: 0f,
                damageFalloff: false
            );
            }
            finally
            {
                ArmorTraitSystemshockContext.End();
            }
            state.StartCooldown(cooldownTicks);

            return true;
        }

    }
}

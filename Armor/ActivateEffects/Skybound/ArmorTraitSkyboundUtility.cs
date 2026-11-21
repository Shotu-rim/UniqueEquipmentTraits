
using RimWorld;
using System;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSkyboundUtility
    {
        public static float GetJumpRange(
            QualityCategory quality)
        {
            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return 0f;

            return 10f + 120f * (strength - 0.30f);
        }

        public static int GetCooldownTicks(
            QualityCategory quality)
        {
            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return 0;

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            float seconds = 60f - 100f * strength;

            return Math.Max(
                1,
                (int)Math.Round(seconds * 60f));
        }

        public static bool TryJump(
            Pawn pawn,
            IntVec3 destination,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (pawn == null ||
                pawn.Dead ||
                !pawn.Spawned ||
                pawn.Map == null ||
                state == null)
            {
                return false;
            }

            if (state.IsOnCooldown)
                return false;

            float range = GetJumpRange(quality);

            if (range <= 0f)
                return false;

            Map map = pawn.Map;

            if (!JumpUtility.ValidJumpTarget(
                pawn, map, destination))
            {
                return false;
            }

            if (!JumpUtility.CanHitTargetFrom(
                pawn,
                pawn.Position,
                new LocalTargetInfo(destination),
                range))
            {
                return false;
            }

            VerbProperties verbProps =
                new VerbProperties();

            bool success = JumpUtility.DoJump(
                pawn,
                new LocalTargetInfo(destination),
                null,
                verbProps);

            if (!success)
                return false;

            state.StartCooldown(
                GetCooldownTicks(quality));

            return true;
        }

    }
}

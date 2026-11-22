
using System;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitOverdriveSpeedUtility
    {
        public static float GetSpeedMultiplier(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return 1f;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Overdrive");

            if (def == null)
                return 1f;

            Hediff_Overdrive overdrive =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(def)
                as Hediff_Overdrive;

            if (overdrive == null ||
                overdrive.TicksRemaining <= 0 ||
                overdrive.SpeedBonus <= 0f)
            {
                return 1f;
            }

            return 1f + overdrive.SpeedBonus;
        }

        public static int AdjustAttackTicks(
            int ticks,
            Pawn pawn)
        {
            if (ticks <= 0)
                return ticks;

            float multiplier = GetSpeedMultiplier(pawn);

            if (multiplier <= 1f)
                return ticks;

            return Math.Max(
                1,
                (int)Math.Round(ticks / multiplier));
        }
    }
}

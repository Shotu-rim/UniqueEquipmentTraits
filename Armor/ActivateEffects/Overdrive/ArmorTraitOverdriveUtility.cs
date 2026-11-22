
using RimWorld;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitOverdriveUtility
    {
        private const int CooldownTicks = 600;

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
                    return 12 * 60;

                case QualityCategory.Legendary:
                    return 16 * 60;

                default:
                    return 0;
            }
        }

        public static int GetExhaustedTicks(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 35 * 60;

                case QualityCategory.Legendary:
                    return 30 * 60;

                default:
                    return 0;
            }
        }


        public static float GetSpeedBonus(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.40f;

                case QualityCategory.Legendary:
                    return 0.50f;

                default:
                    return 0f;
            }
        }


        public static float GetExhaustedPenalty(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.40f;

                case QualityCategory.Legendary:
                    return 0.30f;

                default:
                    return 0f;
            }
        }

        public static bool HasBlockingHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return false;

            HediffDef overdriveDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Overdrive");

            HediffDef exhaustedDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Exhausted");

            return
                (overdriveDef != null &&
                 pawn.health.hediffSet
                     .GetFirstHediffOfDef(overdriveDef) != null)
                ||
                (exhaustedDef != null &&
                 pawn.health.hediffSet
                     .GetFirstHediffOfDef(exhaustedDef) != null);
        }

        public static bool CanActivate(
            Pawn pawn,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                !pawn.Spawned ||
                pawn.health == null ||
                state == null ||
                state.IsEffectActive ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            if (GetDurationTicks(quality) <= 0)
                return false;

            if (HasBlockingHediff(pawn))
                return false;

            return true;
        }

        public static bool TryActivate(
            Pawn pawn,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanActivate(pawn, quality, state))
                return false;

            HediffDef overdriveDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Overdrive");

            if (overdriveDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_Overdrive");
                return false;
            }

            Hediff_Overdrive overdrive =
                HediffMaker.MakeHediff(
                    overdriveDef,
                    pawn) as Hediff_Overdrive;

            if (overdrive == null)
            {
                Log.Error(
                    "[UET] UET_Overdrive has an invalid hediffClass.");
                return false;
            }

            int durationTicks =
                GetDurationTicks(quality);

            int exhaustedTicks =
                GetExhaustedTicks(quality);

            float speedBonus =
                GetSpeedBonus(quality);

            float exhaustedPenalty =
                GetExhaustedPenalty(quality);

            if (speedBonus <= 0f ||
                exhaustedTicks <= 0)
            {
                return false;
            }

            if (!state.TryConsumeCharge())
                return false;

            overdrive.Activate(
                speedBonus,
                durationTicks,
                exhaustedPenalty,
                exhaustedTicks);

            pawn.health.AddHediff(overdrive);

            state.StartActivation(
                durationTicks,
                CooldownTicks);

            return true;
        }
    }
}

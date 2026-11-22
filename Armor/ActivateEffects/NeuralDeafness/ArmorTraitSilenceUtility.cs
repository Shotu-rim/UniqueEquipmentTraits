
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSilenceUtility
    {
        private const float Range = 32f;
        private const int DurationTicks = 7200;

        public static float GetRange()
        {
            return Range;
        }

        public static bool CanActivate(
            Pawn caster,
            Pawn target,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                target == null ||
                target.Dead ||
                !target.Spawned ||
                target.Map != caster.Map ||
                target.health == null ||
                target.health.hediffSet == null ||
                quality != QualityCategory.Legendary ||
                state == null ||
                !state.HasAvailableCharge)
            {
                return false;
            }

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
            if (!CanActivate(caster, target, quality, state))
                return false;

            HediffDef silenceDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Silence");

            if (silenceDef == null)
            {
                Log.Error("[UET] Missing HediffDef: UET_Silence");
                return false;
            }

            if (!state.TryConsumeCharge())
                return false;

            Hediff existingSilence =
                target.health.hediffSet.GetFirstHediffOfDef(
                    silenceDef);

            if (existingSilence != null)
            {
                HediffComp_Disappears disappears =
                    existingSilence.TryGetComp<HediffComp_Disappears>();

                if (disappears != null)
                    disappears.ticksToDisappear = DurationTicks;
            }
            else
            {
                Hediff silence =
                    HediffMaker.MakeHediff(
                        silenceDef,
                        target);

                target.health.AddHediff(silence);
            }

            Messages.Message(
                "UET: Silence applied to " +
                target.LabelShort,
                MessageTypeDefOf.NeutralEvent,
                false);

            return true;
        }
    }
}

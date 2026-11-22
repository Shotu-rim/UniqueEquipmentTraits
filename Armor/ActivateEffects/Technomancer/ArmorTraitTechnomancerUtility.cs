
using System.Collections.Generic;
using RimWorld;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitTechnomancerUtility
    {
        private const int CooldownTicks = 600;
        private const float EffectMultiplier = 1.5f;

        public static int GetCooldownTicks()
        {
            return CooldownTicks;
        }

        public static float GetRadius(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 9f;

                case QualityCategory.Legendary:
                    return 12f;

                default:
                    return 0f;
            }
        }

        public static int GetDurationTicks(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 20 * 60;

                case QualityCategory.Legendary:
                    return 30 * 60;

                default:
                    return 0;
            }
        }

        public static float GetCapacityBonus(
            QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 0.30f * EffectMultiplier;

                case QualityCategory.Legendary:
                    return 0.35f * EffectMultiplier;

                default:
                    return 0f;
            }
        }

        private static bool IsEligibleMech(Pawn pawn)
        {
            if (pawn == null ||
                pawn.Dead ||
                !pawn.Spawned ||
                pawn.health == null ||
                pawn.Faction != Faction.OfPlayer ||
                pawn.RaceProps == null ||
                !pawn.RaceProps.IsMechanoid)
            {
                return false;
            }

            // Only mechs currently controlled by a mechanitor are eligible
            Pawn overseer = pawn.GetOverseer();

            if (overseer == null ||
                overseer.Dead ||
                overseer.Faction != Faction.OfPlayer)
            {
                return false;
            }

            return true;
        }

        public static bool CanActivate(
            Pawn caster,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                state == null ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            return GetRadius(quality) > 0f &&
                GetDurationTicks(quality) > 0 &&
                GetCapacityBonus(quality) > 0f;
        }

        public static bool TryActivate(
            Pawn caster,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanActivate(caster, quality, state))
                return false;

            HediffDef technomancerDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Technomancer");

            if (technomancerDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_Technomancer");
                return false;
            }
            if (technomancerDef.hediffClass == null ||
                !typeof(Hediff_Technomancer).IsAssignableFrom(
                technomancerDef.hediffClass))
            {
                Log.Error(
                    "[UET] Invalid hediffClass for UET_Technomancer.");
                return false;
            }
            float radius = GetRadius(quality);
            int duration = GetDurationTicks(quality);
            float bonus = GetCapacityBonus(quality);

            List<Pawn> targets = new List<Pawn>();

            foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned)
            {
                if (!IsEligibleMech(pawn))
                    continue;

                if (!caster.Position.InHorDistOf(
                    pawn.Position,
                    radius))
                {
                    continue;
                }

                targets.Add(pawn);
            }

            // An empty pulse does not consume a charge
            if (targets.Count == 0)
                return false;

            if (!state.TryConsumeCharge())
                return false;

            foreach (Pawn target in targets)
            {
                Hediff_Technomancer existing =
                    target.health.hediffSet
                        .GetFirstHediffOfDef(
                            technomancerDef)
                    as Hediff_Technomancer;

                if (existing != null)
                {
                    // Refresh without stacking
                    existing.Activate(bonus, duration);
                    continue;
                }

                Hediff_Technomancer hediff =
                    HediffMaker.MakeHediff(
                        technomancerDef,
                        target) as Hediff_Technomancer;

                if (hediff == null)
                {
                    Log.Error(
                        "[UET] Invalid hediffClass for UET_Technomancer.");
                    continue;
                }

                target.health.AddHediff(hediff);
                hediff.Activate(bonus, duration);
            }

            state.StartCooldown(CooldownTicks);

            return true;
        }
    }
}

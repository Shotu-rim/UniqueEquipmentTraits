
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class ToxicEffect : UETEffect
    {
        // Roboczy współczynnik zatrucia.
        private const float ToxicCoefficient = 0.015f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.health == null)
                return;

            // Toxic działa wyłącznie na organizmy biologiczne.
            if (victim.RaceProps == null ||
                !victim.RaceProps.IsFlesh ||
                victim.RaceProps.IsMechanoid)
                return;

            float damage = context.DamageAmount;
            float effectStrength = context.EffectStrength;

            if (damage <= 0f || effectStrength <= 0f)
                return;

            // Zabezpieczenie przed dzieleniem przez zero.
            float bodySize = Mathf.Max(0.1f, victim.BodySize);

            // Odporność na zatrucie.
            float resistance = Mathf.Clamp01(
                victim.GetStatValue(StatDefOf.ToxicResistance)
            );

            // Dawka przed zastosowaniem nasycenia.
            float rawDose =
                damage *
                ToxicCoefficient *
                (1f - resistance) /
                bodySize *
                (10f * effectStrength);

            // Pułap dawki zależny od jakości i rozmiaru.
            float capacity =
                (0.18f + 0.2f * effectStrength) /
                Mathf.Sqrt(Mathf.Max(1f, bodySize));

            if (rawDose <= 0f || capacity <= 0f)
                return;

            // Zatwierdzony wzór miękkiego nasycenia.
            float toxicIncrease =
                rawDose * capacity / (rawDose + capacity);

            if (toxicIncrease <= 0f)
                return;

            HediffDef toxicDef = HediffDefOf.ToxicBuildup;

            if (toxicDef == null)
            {
                Log.Error("[UET] Missing HediffDef: ToxicBuildup");
                return;
            }

            // ToxicBuildup obejmuje cały organizm.
            Hediff toxic = victim.health.hediffSet
                .GetFirstHediffOfDef(toxicDef);

            float previousSeverity =
                toxic != null ? toxic.Severity : 0f;

            if (toxic == null)
            {
                toxic = HediffMaker.MakeHediff(
                    toxicDef,
                    victim
                );

                // Ustawiamy Severity przed dodaniem Hediffa.
                toxic.Severity = toxicIncrease;
                victim.health.AddHediff(toxic);
            }
            else
            {
                toxic.Severity += toxicIncrease;
            }


            context.Report(
                $"{context.Attacker.LabelShort} increased " +
                $"{context.Victim.LabelShort}'s toxic buildup " +
                $"by {toxicIncrease:P0}, raising it from " +
                $"{previousSeverity:P0} to {toxic.Severity:P0}.");

        }
    }
}


using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class SerratedEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.Victim.health == null ||
                context.Victim.Dead)
                return;

            Pawn victim = context.Victim;

            if (!victim.health.CanBleed)
                return;

            float bleedingAfterAttack =
                victim.health.hediffSet.BleedRateTotal;

            float newBleeding = Mathf.Max(
                0f,
                bleedingAfterAttack - context.BleedingBeforeAttack);

            if (newBleeding <= 0f)
                return;

            float bonus = newBleeding * context.EffectStrength;

            if (bonus <= 0f)
                return;

            HediffDef serratedDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_SerratedBleeding");

            if (serratedDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_SerratedBleeding");
                return;
            }

            Hediff existing =
                victim.health.hediffSet.GetFirstHediffOfDef(
                    serratedDef);

            if (existing != null)
            {
                existing.Severity += bonus;
            }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(
                    serratedDef, victim);

                hediff.Severity = bonus;
                victim.health.AddHediff(hediff);
            }



            context.Report(
                $"{context.Attacker.LabelShort} inflicted additional bleeding on " +
                $"{context.Victim.LabelShort} with a serrated strike, " +
                $"increasing the bleeding rate by {bonus:P1}/day. " +
                $"Bleeding before the attack: {context.BleedingBeforeAttack:P1}/day; " +
                $"bleeding caused by the attack: +{newBleeding:P1}/day; " +
                $"total bleeding: {victim.health.hediffSet.BleedRateTotal:P1}/day.");


        }
    }
}

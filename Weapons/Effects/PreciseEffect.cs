
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class PreciseEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.Victim.health == null ||
                context.Victim.Dead)
                return;

            Pawn victim = context.Victim;
            float multiplier = context.EffectStrength * 2f;

            if (multiplier <= 0f)
                return;

            if (victim.RaceProps.IsMechanoid)
                multiplier *= 0.25f;

            float totalBonus = 0f;

            List<string> damageDetails =
                new List<string>();

            foreach (KeyValuePair<BodyPartRecord, float> entry
                in context.PartDamage)
            {
                if (victim.Dead)
                    break;

                BodyPartRecord part = entry.Key;
                float originalDamage = entry.Value;

                if (part == null || originalDamage <= 0f)
                    continue;

                if (!victim.health.hediffSet.HasBodyPart(part))
                    continue;

                if (!context.PartInjuryDefs.TryGetValue(
                    part, out HediffDef injuryDef))
                    continue;

                if (injuryDef == null)
                    continue;

                float remainingHealth =
                    victim.health.hediffSet.GetPartHealth(part);

                if (remainingHealth <= 0f)
                    continue;

                float bonusDamage = Mathf.Min(
                    originalDamage * multiplier,
                    remainingHealth);

                if (bonusDamage <= 0f)
                    continue;

                Hediff_Injury bonusInjury =
                    HediffMaker.MakeHediff(
                        injuryDef, victim, part) as Hediff_Injury;

                if (bonusInjury == null)
                    continue;

                bonusInjury.Severity = bonusDamage;

                victim.health.AddHediff(bonusInjury, part);

                totalBonus += bonusDamage;

                damageDetails.Add(
                    $"{part.Label}: +{bonusDamage:0.##}");
            }

            if (totalBonus > 0f)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} struck " +
                    $"{context.Victim.LabelShort} with exceptional precision, " +
                    $"dealing {totalBonus:0.##} additional damage " +
                    $"({string.Join("; ", damageDetails)}).");
            }
        }
    }
}

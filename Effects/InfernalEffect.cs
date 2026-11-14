
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using static Verse.DamageWorker;

namespace UniqueEquipmentTraits.Effects
{
    public class InfernalEffect : UETEffect
    {
        private const float BurnMultiplier = 10f;
        private const float DamageScaling = 0.5f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.health == null)
                return;

            // Infernal nie działa na mechanoidy.
            if (victim.RaceProps == null ||
                victim.RaceProps.IsMechanoid)
                return;

            if (context.HitParts == null ||
                context.HitParts.Count == 0)
                return;

            DamageDef burnDef = DamageDefOf.Burn;

            if (burnDef == null)
            {
                Log.Error("[UET] Missing DamageDef: Burn");
                return;
            }

            // Base damage counter
            float baseBurnDamage =
                context.EffectStrength * BurnMultiplier;

            // Easy growth
            float originalDamage =
                Math.Max(0f, context.DamageAmount);

            float bonusDamage =
                DamageScaling *
                (float)Math.Sqrt(
                    Math.Max(0f, originalDamage - 1f));

            float burnDamage =
                baseBurnDamage + bonusDamage;

            if (burnDamage <= 0f)
                return;

            int partsAffected = 0;
            float totalDamageDealt = 0f;

            List<string> burnedParts =
                new List<string>();

            foreach (BodyPartRecord part in context.HitParts)
            {
                if (victim.Dead)
                    break;

                if (part == null)
                    continue;

                // Ignore missing
                if (victim.health.hediffSet.PartIsMissing(part))
                    continue;

                DamageInfo dinfo = new DamageInfo(
                    burnDef,
                    burnDamage,
                    0f,
                    -1f,
                    context.Attacker
                );

                // Only hit
                dinfo.SetHitPart(part);

                dinfo.SetIgnoreArmor(true);

                dinfo.SetAllowDamagePropagation(false);

                DamageResult result =
                    victim.TakeDamage(dinfo);

                if (result != null &&
                    result.totalDamageDealt > 0f)
                {
                    partsAffected++;

                    totalDamageDealt +=
                        result.totalDamageDealt;

                    burnedParts.Add(part.LabelCap);
                }
            }

            if (partsAffected > 0)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} inflicted burns on " +
                    $"{victim.LabelShort}, affecting {partsAffected} " +
                    $"body part(s) for {totalDamageDealt:F1} total damage " +
                    $"({burnDamage:F1} base damage per part): " +
                    $"{string.Join(", ", burnedParts)}.");
            }
        }
    }
}

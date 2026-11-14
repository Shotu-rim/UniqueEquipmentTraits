
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using static Verse.DamageWorker;

namespace UniqueEquipmentTraits.Effects
{
    public class GlacialEffect : UETEffect
    {
        private const float FrostbiteMultiplier = 10f;
        private const float DamageScaling = 0.5f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.health == null)
                return;

            // Mechs are safe
            if (victim.RaceProps == null ||
                victim.RaceProps.IsMechanoid)
                return;

            if (context.HitParts == null ||
                context.HitParts.Count == 0)
                return;

            DamageDef frostbiteDef =
                DefDatabase<DamageDef>.GetNamedSilentFail("Frostbite");

            if (frostbiteDef == null)
            {
                Log.Error("[UET] Missing DamageDef: Frostbite");
                return;
            }

            // Base damage based on EffectStrength.
            float baseFrostbiteDamage =
                context.EffectStrength * FrostbiteMultiplier;

            // Total damage
            float originalDamage = Math.Max(0f, context.DamageAmount);

            // Incremental increase of damage
            float bonusDamage =
                DamageScaling *
                (float)Math.Sqrt(Math.Max(0f, originalDamage - 1f));

            // All damage the same on each hit part
            float frostbiteDamage =
                baseFrostbiteDamage + bonusDamage;

            if (frostbiteDamage <= 0f)
                return;

            int partsAffected = 0;
            float totalDamageDealt = 0f;

            List<string> affectedParts = new List<string>();

            foreach (BodyPartRecord part in context.HitParts)
            {
                if (victim.Dead)
                    break;

                if (part == null)
                    continue;

                // No attack on missing parts
                if (victim.health.hediffSet.PartIsMissing(part))
                    continue;

                DamageInfo dinfo = new DamageInfo(
                    frostbiteDef,
                    frostbiteDamage,
                    0f,
                    -1f,
                    context.Attacker
                );

                // Only hit part
                dinfo.SetHitPart(part);

                dinfo.SetIgnoreArmor(true);

                // No propagation on other body parts
                dinfo.SetAllowDamagePropagation(false);

                DamageResult result = victim.TakeDamage(dinfo);

                if (result != null && result.totalDamageDealt > 0f)
                {
                    partsAffected++;

                    totalDamageDealt += result.totalDamageDealt;

                    affectedParts.Add(part.LabelCap);
                }
            }

            if (partsAffected > 0)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} inflicted frostbite on " +
                    $"{victim.LabelShort}, affecting {partsAffected} " +
                    $"body part(s) for {totalDamageDealt:F1} total damage " +
                    $"({frostbiteDamage:F1} base damage per part): " +
                    $"{string.Join(", ", affectedParts)}.");
            }
        }
    }
}

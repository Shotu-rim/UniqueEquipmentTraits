
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Effects;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Applies bonus injury damage from Hawkeye armor traits.
    /// Uses the original melee attack damage snapshot,
    /// independently of weapon trait effects.
    /// </summary>
    internal static class ArmorTraitCriticalEffect
    {
        internal static void Apply(MeleeAttackContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Attacker.Dead ||
                context.Victim.Dead ||
                context.Victim.health == null ||
                !context.DamageDealt ||
                context.OriginalPartDamage == null)
            {
                return;
            }

            float criticalChance;
            float criticalDamageBonus;

            ArmorTraitCriticalUtility.GetCriticalStats(
                context.Attacker,
                out criticalChance,
                out criticalDamageBonus);

            if (criticalChance <= 0f ||
                criticalDamageBonus <= 0f)
            {
                return;
            }

            if (!Rand.Chance(Mathf.Clamp01(criticalChance)))
                return;

            Pawn victim = context.Victim;
            float totalBonus = 0f;

            foreach (KeyValuePair<BodyPartRecord, float> entry
                in context.OriginalPartDamage)
            {
                if (victim.Dead)
                    break;

                BodyPartRecord part = entry.Key;
                float originalDamage = entry.Value;

                if (part == null || originalDamage <= 0f)
                    continue;

                if (!victim.health.hediffSet.HasBodyPart(part))
                    continue;

                if (context.OriginalPartInjuryDefs == null ||
                    !context.OriginalPartInjuryDefs.TryGetValue(
                        part, out HediffDef injuryDef) ||
                    injuryDef == null)
                {
                    continue;
                }

                float remainingHealth =
                    victim.health.hediffSet.GetPartHealth(part);

                if (remainingHealth <= 0f)
                    continue;

                float bonusDamage = Mathf.Min(
                    originalDamage * criticalDamageBonus,
                    remainingHealth);

                if (bonusDamage <= 0f)
                    continue;

                Hediff_Injury bonusInjury =
                    HediffMaker.MakeHediff(
                        injuryDef,
                        victim,
                        part) as Hediff_Injury;

                if (bonusInjury == null)
                    continue;

                bonusInjury.Severity = bonusDamage;

                victim.health.AddHediff(
                    bonusInjury,
                    part);

                totalBonus += bonusDamage;
            }
            ArmorTraitDef hawkeye =
                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                "UET_Hawkeye");


            if (totalBonus > 0f && hawkeye != null)
            {
                string description =
                    context.Attacker.LabelShort +
                    " spotted an opening in " +
                    victim.LabelShort +
                    "'s defenses and delivered a devastating strike " +
                    "dealing additional " + totalBonus.ToString("0.##") +
                    " damage.";

                UETCombatLog.Record(
                    hawkeye,
                    context.Attacker,
                    description,
                    victim);
            }
                               
            }
        }
    }
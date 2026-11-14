
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Traits
{
    public static class BloodseekerUtility
    {
        public static float TryHeal(
            Pawn attacker,
            Pawn victim,
            ThingWithComps weapon,
            float bleedingBeforeAttack)
        {
            if (attacker == null || victim == null || weapon == null)
                return 0f;

            if (attacker.Dead || attacker.health == null ||
                victim.health == null)
                return 0f;

            CompQuality qualityComp =
                weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return 0f;

            // Alive victim - bleed after attack, dead victim - before attack (balance related)
            float bleeding = victim.Dead
                ? bleedingBeforeAttack
                : victim.health.hediffSet.BleedRateTotal;

            if (bleeding <= 0f)
                return 0f;

            float qualityFactor =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            if (qualityFactor <= 0f)
                return 0f;

            List<Hediff_Injury> injuries =
                new List<Hediff_Injury>();

            float missingHP = 0f;

            foreach (Hediff hediff in
                attacker.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;

                if (injury == null ||
                    injury.IsPermanent() ||
                    injury.Severity <= 0f ||
                    injury.Part == null)
                    continue;

                injuries.Add(injury);
                missingHP += injury.Severity;
            }

            if (injuries.Count == 0)
                return 0f;

            float bleedingMultiplier =
                Mathf.Clamp(bleeding, 0f, 3f);

            float bleedingFactor =
                Mathf.Pow(bleedingMultiplier / 3f, 0.8f);

            float healingPool = missingHP * bleedingFactor;
            float totalHealed = 0f;

            float basePartLimit = 0.4f;

            if (qualityComp.Quality == QualityCategory.Legendary)
                basePartLimit = 0.5f;

            float partLimitFactor =
                basePartLimit * (1f + qualityFactor);

            Dictionary<BodyPartRecord, float> healedByPart =
                new Dictionary<BodyPartRecord, float>();

            injuries.Shuffle();

            foreach (Hediff_Injury injury in injuries)
            {
                if (healingPool <= 0f)
                    break;

                BodyPartRecord part = injury.Part;

                float maxPartHealing =
                    part.def.GetMaxHealth(attacker) *
                    partLimitFactor;

                float alreadyHealed = 0f;
                healedByPart.TryGetValue(
                    part,
                    out alreadyHealed);

                float remainingPartHealing = Mathf.Max(
                    0f,
                    maxPartHealing - alreadyHealed
                );

                if (remainingPartHealing <= 0f)
                    continue;

                float amount = Mathf.Min(
                    healingPool,
                    injury.Severity,
                    remainingPartHealing
                );

                if (amount <= 0f)
                    continue;

                injury.Heal(amount);

                totalHealed += amount;
                healingPool -= amount;

                healedByPart[part] = alreadyHealed + amount;

                if (injury.ShouldRemove)
                    attacker.health.RemoveHediff(injury);
            }

            return totalHealed;
        }
    }
}


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
            float bleedingBeforeAttack,
            BloodseekerHealingReport report = null)
        {
            if (attacker == null || victim == null || weapon == null)
            {
                if (report != null)
                    report.SkipReason = "Invalid attacker, victim or weapon";

                return 0f;
            }

            if (attacker.Dead || attacker.health == null ||
                victim.health == null)
            {
                if (report != null)
                    report.SkipReason = "Invalid pawn health";

                return 0f;
            }

            CompQuality qualityComp =
                weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
            {
                if (report != null)
                    report.SkipReason = "Missing quality component";

                return 0f;
            }

            // Żywa ofiara: krwawienie po trafieniu.
            // Martwa ofiara: krwawienie sprzed trafienia.
            float bleeding = victim.Dead
                ? bleedingBeforeAttack
                : victim.health.hediffSet.BleedRateTotal;

            if (report != null)
            {
                report.BleedingBeforeAttack = bleedingBeforeAttack;
                report.BleedingAfterAttack =
                    victim.health.hediffSet.BleedRateTotal;
                report.BleedingUsed = bleeding;
            }

            if (bleeding <= 0f)
            {
                if (report != null)
                    report.SkipReason = "Victim not bleeding";

                return 0f;
            }

            float qualityFactor =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            if (report != null)
                report.EffectStrength = qualityFactor;

            if (qualityFactor <= 0f)
            {
                if (report != null)
                    report.SkipReason = "Invalid quality factor";

                return 0f;
            }

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

                // Diagnostyka: stan ran przed leczeniem.
                if (report != null)
                {
                    BodyPartRecord part = injury.Part;

                    if (!report.InjuriesByPart.TryGetValue(
                        part,
                        out BloodseekerHealingReport.PartInjuryInfo info))
                    {
                        info = new BloodseekerHealingReport.PartInjuryInfo();
                        report.InjuriesByPart.Add(part, info);
                    }

                    info.WoundCount++;
                    info.MissingHP += injury.Severity;
                }
            }

            if (report != null)
            {
                report.InjuryCount = injuries.Count;
                report.MissingHP = missingHP;
            }

            if (injuries.Count == 0)
            {
                if (report != null)
                    report.SkipReason = "No healable injuries";

                return 0f;
            }

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

            if (report != null)
            {
                report.BleedingFactor = bleedingFactor;
                report.InitialHealingPool = healingPool;
                report.PartLimitFactor = partLimitFactor;
            }

            Dictionary<BodyPartRecord, float> healedByPart =
                new Dictionary<BodyPartRecord, float>();

            // Zachowujemy istniejące losowanie kolejności.
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

            if (report != null)
            {
                report.TotalHealed = totalHealed;
                report.UnusedHealingPool = healingPool;

                foreach (KeyValuePair<BodyPartRecord, float> entry
                    in healedByPart)
                {
                    report.HealedByPart.Add(
                        entry.Key,
                        entry.Value);
                }

                if (totalHealed <= 0f)
                    report.SkipReason =
                        "Healing prevented by part limits";
            }

            return totalHealed;
        }
    }
}


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
            Log.Message("[UET] Bloodseeker UTILITY ENTERED");

            if (attacker == null || victim == null || weapon == null)
            {
                Log.Message(
                    "[UET] Bloodseeker SKIPPED | " +
                    "Reason: Null attacker, victim or weapon");
                return 0f;
            }

            if (attacker.Dead || attacker.health == null ||
                victim.health == null)
            {
                Log.Message(
                    "[UET] Bloodseeker SKIPPED | " +
                    "Reason: Invalid pawn health");
                return 0f;
            }


            CompQuality qualityComp =
                weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
            {
                Log.Message(
                    "[UET] Bloodseeker SKIPPED | " +
                    "Reason: Missing quality component");
                return 0f;
            }

            // Żywa ofiara: krwawienie po trafieniu.
            // Martwa ofiara: krwawienie sprzed trafienia.
            float bleeding = victim.Dead
                ? bleedingBeforeAttack
                : victim.health.hediffSet.BleedRateTotal;

            // Diagnostyka krwawienia aktualnych ran.
            float injuryBleeding = 0f;

            foreach (Hediff hediff in
                victim.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;

                if (injury != null)
                    injuryBleeding += injury.BleedRate;
            }

            Log.Message(
                $"[UET] Bloodseeker BLEED CHECK | " +
                $"Victim: {victim.LabelShort} | " +
                $"Downed: {victim.Downed} | " +
                $"Dead: {victim.Dead} | " +
                $"BleedingBeforeAttack: {bleedingBeforeAttack:F3} | " +
                $"BleedRateTotal: " +
                $"{victim.health.hediffSet.BleedRateTotal:F3} | " +
                $"InjuryBleeding: {injuryBleeding:F3} | " +
                $"BleedingUsed: {bleeding:F3}");

            if (bleeding <= 0f)
            {
                Log.Message(
                    "[UET] Bloodseeker SKIPPED | " +
                    "Reason: Victim not bleeding");
                return 0f;
            }

            float qualityFactor =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            if (qualityFactor <= 0f)
            {
                Log.Message(
                    "[UET] Bloodseeker SKIPPED | " +
                    "Reason: Invalid quality factor");
                return 0f;
            }

            // Zbieramy wszystkie kwalifikujące się rany
            // z całego ciała napastnika.
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
            {
                Log.Message(
                    $"[UET] Bloodseeker SKIPPED | " +
                    $"Reason: No healable injuries | " +
                    $"Attacker: {attacker.LabelShort}");
                return 0f;
            }

            // 100% krwawienia = mnożnik 1.
            // 150% i więcej = maksymalny mnożnik 1.5.
            float bleedingMultiplier =
                Mathf.Clamp(bleeding, 0f, 1.5f);

            float healing =
                missingHP * qualityFactor * bleedingMultiplier;

            healing = Mathf.Min(healing, missingHP);

            float healingPool = healing;
            float totalHealed = 0f;

            // Losujemy kolejność ran.
            injuries.Shuffle();

            foreach (Hediff_Injury injury in injuries)
            {
                if (healingPool <= 0f)
                    break;

                float amount =
                    Mathf.Min(healingPool, injury.Severity);

                injury.Heal(amount);

                totalHealed += amount;
                healingPool -= amount;

                if (injury.ShouldRemove)
                    attacker.health.RemoveHediff(injury);
            }

            return totalHealed;
        }
    }
}

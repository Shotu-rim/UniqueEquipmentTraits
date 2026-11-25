using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class CorrosiveEffect : UETEffect
    {
        private const float MechanoidMultiplier = 1.5f;

        // Corrosion damage weighting
        private const float PreArmorDamageWeight = 0.40f;

        private const float PostArmorDamageWeight =
            1f - PreArmorDamageWeight;

        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.EffectStrength <= 0f)
                return;

            Pawn victim = context.Victim;

            // Equipment corrosion does not require armor penetration.
            CorrodeEquipment(context);

            // Additional mechanoid damage still requires actual damage.
            if (victim.Destroyed ||
                victim.Dead ||
                !victim.RaceProps.IsMechanoid ||
                context.DamageAmount <= 0f)
                return;

            float extraDamage =
                context.DamageAmount *
                context.EffectStrength *
                MechanoidMultiplier;

            if (extraDamage <= 0f)
                return;

            // Single extra hit per activation.
            DamageInfo damage = new DamageInfo(
                DamageDefOf.Burn,
                extraDamage,
                armorPenetration: 999f,
                instigator: context.Attacker);

            DamageWorker.DamageResult result =
                victim.TakeDamage(damage);

            if (result.totalDamageDealt > 0f)
            {
                context.Report(
                    $"{context.Attacker.LabelShort} inflicted corrosive damage on " +
                    $"{victim.LabelShort}'s chassis, dealing " +
                    $"{result.totalDamageDealt:F1} damage.");
            }
        }

        /// <summary>
        /// Calculates equipment corrosion using both pre-armor
        /// contact damage and actual post-armor damage.
        /// </summary>
        private static float GetCorrosionBaseDamage(
            UETEffectContext context)
        {
            float postArmorDamage =
                Math.Max(0f, context.DamageAmount);

            // Preserve original behavior if contact data is unavailable.
            if (!context.ContactMade ||
                context.ContactDamageAmount <= 0f)
                return postArmorDamage;

            float preArmorDamage =
                context.ContactDamageAmount;

            return
                preArmorDamage * PreArmorDamageWeight +
                postArmorDamage * PostArmorDamageWeight;
        }

        private static void CorrodeEquipment(
            UETEffectContext context)
        {
            Pawn victim = context.Victim;

            // Prefer physical contact parts, including absorbed hits.
            // Fall back to actual hit parts when contact data is unavailable.
            IReadOnlyList<BodyPartRecord> affectedParts =
                context.ContactMade
                    ? context.ContactParts
                    : context.HitParts;

            if (affectedParts == null ||
                affectedParts.Count == 0)
                return;

            float baseDamage =
                GetCorrosionBaseDamage(context);

            if (baseDamage <= 0f)
                return;

            float share =
                baseDamage *
                context.EffectStrength /
                affectedParts.Count;

            if (share <= 0f)
                return;

            Dictionary<ThingWithComps, float> assigned =
                new Dictionary<ThingWithComps, float>();

            List<ThingWithComps> candidates =
                new List<ThingWithComps>();

            foreach (BodyPartRecord hitPart in affectedParts)
            {
                if (hitPart == null)
                    continue;

                candidates.Clear();

                if (victim.apparel != null)
                {
                    foreach (Apparel apparel
                        in victim.apparel.WornApparel)
                    {
                        if (CanCorrode(apparel) &&
                            apparel.def.apparel != null &&
                            apparel.def.apparel.CoversBodyPart(hitPart))
                        {
                            candidates.Add(apparel);
                        }
                    }
                }

                // Equipment can be selected for manipulation limbs.
                if (victim.equipment != null &&
                    IsManipulationLimb(hitPart))
                {
                    foreach (ThingWithComps equipment
                        in victim.equipment.AllEquipmentListForReading)
                    {
                        if (CanCorrode(equipment) &&
                            !candidates.Contains(equipment))
                        {
                            candidates.Add(equipment);
                        }
                    }
                }

                // Uncovered body parts lose their assigned share.
                if (candidates.Count == 0)
                    continue;

                ThingWithComps selected =
                    candidates[Rand.Range(0, candidates.Count)];

                float previous;

                assigned.TryGetValue(
                    selected,
                    out previous);

                assigned[selected] =
                    previous + share;
            }

            foreach (KeyValuePair<ThingWithComps, float> entry
                in assigned)
            {
                ThingWithComps item = entry.Key;

                if (!CanCorrode(item))
                    continue;

                // Preserve original durability scaling.
                float scaledDamage =
                    entry.Value *
                    (float)Math.Sqrt(item.MaxHitPoints / 100f);

                Apparel apparel = item as Apparel;

                float immunity = apparel != null
                    ? UniqueEquipmentTraits.Core.Armor
                        .ArmorTraitImmunityUtility.GetItemImmunity(
                            apparel,
                            "UET_AcidImmunity")
                    : 0f;

                int wholeDamage;

                if (immunity > 0f)
                {
                    float remainingDamage =
                        scaledDamage * (1f - immunity);

                    var comp = apparel.TryGetComp<
                        UniqueEquipmentTraits.Core.Armor.CompArmorTraits>();

                    wholeDamage = comp != null
                        ? comp.AccumulateImmunityDamage(remainingDamage)
                        : (int)Math.Floor(remainingDamage);
                }
                else
                {
                    // Preserve original corrosion rounding.
                    wholeDamage =
                        (int)Math.Floor(scaledDamage);

                    if (Rand.Value < scaledDamage - wholeDamage)
                        wholeDamage++;
                }

                if (wholeDamage <= 0)
                    continue;

                int hpBefore = item.HitPoints;

                int actualDamage =
                    Math.Min(wholeDamage, hpBefore);

                item.HitPoints =
                    hpBefore - actualDamage;

                context.Report(
                    $"{context.Attacker.LabelShort} corroded " +
                    $"{victim.LabelShort}'s {item.LabelShort}, dealing " +
                    $"{actualDamage} durability damage " +
                    $"({hpBefore} → {item.HitPoints} HP).");

                if (item.HitPoints <= 0 &&
                    !item.Destroyed)
                {
                    item.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private static bool CanCorrode(ThingWithComps item)
        {
            return item != null &&
                   !item.Destroyed &&
                   item.def != null &&
                   item.def.useHitPoints &&
                   item.HitPoints > 0 &&
                   item.MaxHitPoints > 0;
        }

        private static bool IsManipulationLimb(
            BodyPartRecord part)
        {
            foreach (BodyPartRecord descendant
                in part.GetPartAndAllChildParts())
            {
                if (descendant.def != null &&
                    descendant.def.tags != null &&
                    descendant.def.tags.Contains(
                        BodyPartTagDefOf.ManipulationLimbCore))
                    return true;
            }

            return false;
        }
    }
}
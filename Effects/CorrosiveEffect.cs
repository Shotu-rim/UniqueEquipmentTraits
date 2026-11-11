using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class CorrosiveEffect : UETEffect
    {
        private const float MechanoidMultiplier = 1.5f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null ||
                context.DamageAmount <= 0f || context.EffectStrength <= 0f)
                return;

            Pawn victim = context.Victim;

            // The original attack's body parts and damage are used for both effects.
            CorrodeEquipment(context);

            if (victim.Destroyed || victim.Dead || !victim.RaceProps.IsMechanoid)
                return;

            float extraDamage = context.DamageAmount * context.EffectStrength * MechanoidMultiplier;
            if (extraDamage <= 0f)
                return;

            // A single extra hit per activation, never one hit per affected body part.
            // The attacker's weapon is not supplied to avoid counting this as another weapon hit.
            DamageInfo damage = new DamageInfo(
                DamageDefOf.Burn,
                extraDamage,
                armorPenetration: 999f,
                instigator: context.Attacker);

            victim.TakeDamage(damage);
            context.Report("Corrosive: additional mechanoid damage " + extraDamage.ToString("0.##"));
        }

        private static void CorrodeEquipment(UETEffectContext context)
        {
            Pawn victim = context.Victim;
            if (context.HitParts == null || context.HitParts.Count == 0)
                return;

            float share = context.DamageAmount * context.EffectStrength / context.HitParts.Count;
            if (share <= 0f)
                return;

            Dictionary<ThingWithComps, float> assigned = new Dictionary<ThingWithComps, float>();
            List<ThingWithComps> candidates = new List<ThingWithComps>();

            foreach (BodyPartRecord hitPart in context.HitParts)
            {
                if (hitPart == null)
                    continue;

                candidates.Clear();

                if (victim.apparel != null)
                {
                    foreach (Apparel apparel in victim.apparel.WornApparel)
                    {
                        if (CanCorrode(apparel) &&
                            apparel.def.apparel != null &&
                            apparel.def.apparel.CoversBodyPart(hitPart))
                            candidates.Add(apparel);
                    }
                }

                // RimWorld does not record which specific hand holds equipped items.
                // Therefore an equipped item may be selected for either manipulation limb.
                if (victim.equipment != null && IsManipulationLimb(hitPart))
                {
                    foreach (ThingWithComps equipment in victim.equipment.AllEquipmentListForReading)
                    {
                        if (CanCorrode(equipment) && !candidates.Contains(equipment))
                            candidates.Add(equipment);
                    }
                }

                // An uncovered hit part loses its share; no redistribution.
                if (candidates.Count == 0)
                    continue;

                ThingWithComps selected = candidates[Rand.Range(0, candidates.Count)];
                float previous;
                assigned.TryGetValue(selected, out previous);
                assigned[selected] = previous + share;
            }

            foreach (KeyValuePair<ThingWithComps, float> entry in assigned)
            {
                ThingWithComps item = entry.Key;
                if (!CanCorrode(item))
                    continue;

                float scaledDamage = entry.Value *
                    (float)Math.Sqrt(item.MaxHitPoints / 100f);

                int wholeDamage = (int)Math.Floor(scaledDamage);

                if (Rand.Value < scaledDamage - wholeDamage)
                    wholeDamage++;

                if (wholeDamage <= 0)
                    continue;

                int hpBefore = item.HitPoints;
                int actualDamage = Math.Min(wholeDamage, hpBefore);

                item.HitPoints = hpBefore - actualDamage;

                context.Report(
                    $"{context.Attacker.LabelShort} corroded " +
                    $"{item.LabelShort}, reducing its durability by " +
                    $"{actualDamage} HP ({hpBefore} → {item.HitPoints}).");

                if (item.HitPoints <= 0 && !item.Destroyed)
                    item.Destroy(DestroyMode.Vanish);
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

        private static bool IsManipulationLimb(BodyPartRecord part)
        {
            foreach (BodyPartRecord descendant in part.GetPartAndAllChildParts())
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


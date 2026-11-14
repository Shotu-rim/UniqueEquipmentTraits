
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using UniqueEquipmentTraits.Effects;
using UniqueEquipmentTraits.Implants;
using UniqueEquipmentTraits.Mechanoids;

namespace UniqueEquipmentTraits.Traits
{
    internal static class NaniteDisruptionUtility
    {
        private const float MaxDamage = 0.60f;
        private const int MaxInitialParts = 3;
        private const int SpreadMultiplier = 2;
        private const float Epsilon = 0.000001f;
        private const float ImplantDamageMultiplier = 0.75f;
        private const int MaxSpreadParts = 5;

        private sealed class DamageTarget
        {
            internal BodyPartRecord Part;
            internal HediffComp_NaniteDamage Comp;
            internal float Weight;
        }

        internal static NaniteDisruptionResult TryDisrupt(
            Pawn victim,
            float effectStrength)
        {
            if (victim == null ||
                victim.Dead ||
                victim.health == null ||
                effectStrength <= 0f)
                return null;

            if (victim.RaceProps != null &&
                victim.RaceProps.IsMechanoid)
            {
                return TryDisruptMechanoid(
                    victim, effectStrength);
            }

            return TryDisruptImplants(
                victim, effectStrength);
        }

        private static NaniteDisruptionResult TryDisruptImplants(
            Pawn victim,
            float effectStrength)
        {
            List<DamageTarget> targets =
                new List<DamageTarget>();

            foreach (Hediff hediff in
                victim.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_AddedPart implant))
                    continue;

                HediffComp_NaniteDamage comp =
                    implant.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null ||
                    comp.AccumulatedDamage >= MaxDamage)
                    continue;

                targets.Add(new DamageTarget
                {
                    Part = implant.Part,
                    Comp = comp
                });
            }

            if (targets.Count == 0)
                return null;

            int initialCount =
                Mathf.Min(MaxInitialParts, targets.Count);

            float budget =
                effectStrength * initialCount * ImplantDamageMultiplier;

            targets.Shuffle();

            int spreadCount = Mathf.Min(
                targets.Count,
                Mathf.Min(initialCount * 2, MaxSpreadParts));

            targets.RemoveRange(
                spreadCount,
                targets.Count - spreadCount);

            NaniteDisruptionResult result =
                new NaniteDisruptionResult();

            result.IsMechanoid = false;

            DistributeDamage(targets, budget, result);

            return result.Parts.Count > 0 ? result : null;
        }

        private static NaniteDisruptionResult TryDisruptMechanoid(
            Pawn victim,
            float effectStrength)
        {
            HediffDef degradationDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_NaniteMechanicalDegradation");

            if (degradationDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: " +
                    "UET_NaniteMechanicalDegradation");

                return null;
            }

            HediffSet hediffSet =
                victim.health.hediffSet;

            List<BodyPartRecord> eligibleParts =
                new List<BodyPartRecord>();

            foreach (BodyPartRecord part in
                victim.RaceProps.body.AllParts)
            {
                if (hediffSet.PartIsMissing(part))
                    continue;

                Hediff_NaniteMechanicalDegradation existing =
                    FindDegradation(hediffSet, part);

                if (existing != null)
                {
                    HediffComp_NaniteDamage comp =
                        existing.TryGetComp<HediffComp_NaniteDamage>();

                    if (comp == null ||
                        comp.AccumulatedDamage >= MaxDamage)
                        continue;
                }

                eligibleParts.Add(part);
            }

            if (eligibleParts.Count == 0)
                return null;

            int initialCount = Mathf.Min(
                MaxInitialParts, eligibleParts.Count);

            float budget = effectStrength * initialCount;

            eligibleParts.Shuffle();

            int spreadCount = Mathf.Min(
                eligibleParts.Count,
                Mathf.Min(initialCount * 2, MaxSpreadParts));

            List<DamageTarget> targets =
                new List<DamageTarget>();

            for (int i = 0; i < spreadCount; i++)
            {
                BodyPartRecord part = eligibleParts[i];

                Hediff_NaniteMechanicalDegradation degradation =
                    FindDegradation(hediffSet, part);

                if (degradation == null)
                {
                    degradation =
                        HediffMaker.MakeHediff(
                            degradationDef, victim, part)
                        as Hediff_NaniteMechanicalDegradation;

                    if (degradation == null)
                        continue;

                    victim.health.AddHediff(
                        degradation, part);
                }

                HediffComp_NaniteDamage comp =
                    degradation.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null ||
                    comp.AccumulatedDamage >= MaxDamage)
                    continue;

                targets.Add(new DamageTarget
                {
                    Part = part,
                    Comp = comp
                });
            }

            NaniteDisruptionResult result =
                new NaniteDisruptionResult();

            result.IsMechanoid = true;

            DistributeDamage(targets, budget, result);

            return result.Parts.Count > 0 ? result : null;
        }

        private static void DistributeDamage(
            List<DamageTarget> targets,
            float budget,
            NaniteDisruptionResult result)
        {
            if (targets.Count == 0 || budget <= 0f)
                return;

            float[] allocations = new float[targets.Count];

            // Weight per activation
            foreach (DamageTarget target in targets)
            {
                target.Weight = Rand.Range(0.1f, 1f);
            }

            float remaining = budget;

            // Targets different parts and spends effect on them, to the limit, unspent points are used on another part
            for (int iteration = 0;
                iteration < targets.Count + 1 &&
                remaining > Epsilon;
                iteration++)
            {
                float totalWeight = 0f;

                for (int i = 0; i < targets.Count; i++)
                {
                    float capacity =
                        MaxDamage -
                        targets[i].Comp.AccumulatedDamage -
                        allocations[i];

                    if (capacity > Epsilon)
                        totalWeight += targets[i].Weight;
                }

                if (totalWeight <= 0f)
                    break;

                float distributed = 0f;

                for (int i = 0; i < targets.Count; i++)
                {
                    DamageTarget target = targets[i];

                    float capacity =
                        MaxDamage -
                        target.Comp.AccumulatedDamage -
                        allocations[i];

                    if (capacity <= Epsilon)
                        continue;

                    float share =
                        remaining *
                        target.Weight / totalWeight;

                    float amount = Mathf.Min(
                        share, capacity);

                    allocations[i] += amount;
                    distributed += amount;
                }

                if (distributed <= Epsilon)
                    break;

                remaining = Mathf.Max(
                    0f, remaining - distributed);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                float amount = allocations[i];

                if (amount <= Epsilon)
                    continue;

                DamageTarget target = targets[i];

                float before = target.Comp.AccumulatedDamage;

                target.Comp.ApplyDamage(amount);

                float after = target.Comp.AccumulatedDamage;
                float applied = after - before;

                if (applied <= Epsilon)
                    continue;

                if (target.Comp.parent is
                    Hediff_NaniteMechanicalDegradation degradation)
                {
                    degradation.Severity = after;
                }

                string partName =
                    target.Comp.parent.Label;

                if (target.Part != null)
                    partName = target.Part.Label;

                result.Parts.Add(
                    new NaniteDisruptionPartResult
                    {
                        PartName = partName,
                        AppliedPercent = applied * 100f,
                        TotalPercent = after * 100f
                    });
            }
        }

        private static Hediff_NaniteMechanicalDegradation
            FindDegradation(
                HediffSet hediffSet,
                BodyPartRecord part)
        {
            foreach (Hediff hediff in hediffSet.hediffs)
            {
                if (hediff.Part != part)
                    continue;

                if (hediff is
                    Hediff_NaniteMechanicalDegradation degradation)
                {
                    return degradation;
                }
            }

            return null;
        }
    }
}

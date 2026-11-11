
using System.Collections.Generic;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Effects;
using UniqueEquipmentTraits.Implants;
using UniqueEquipmentTraits.Mechanoids;

namespace UniqueEquipmentTraits.Traits
{
    internal static class NaniteDisruptionUtility
    {
        private const float MaxDamage = 0.60f;

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
                    victim,
                    effectStrength);
            }

            return TryDisruptImplant(
                victim,
                effectStrength);
        }

        private static NaniteDisruptionResult TryDisruptImplant(
            Pawn victim,
            float effectStrength)
        {
            List<HediffComp_NaniteDamage> eligibleImplants =
                new List<HediffComp_NaniteDamage>();

            foreach (Hediff hediff in victim.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_AddedPart implant))
                    continue;

                HediffComp_NaniteDamage comp =
                    implant.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null)
                    continue;

                if (comp.AccumulatedDamage >= MaxDamage)
                    continue;

                eligibleImplants.Add(comp);
            }

            if (eligibleImplants.Count == 0)
                return null;

            HediffComp_NaniteDamage selected =
                eligibleImplants.RandomElement();

            float damageBefore = selected.AccumulatedDamage;

            selected.ApplyDamage(effectStrength);

            float damageAfter = selected.AccumulatedDamage;
            float damageApplied = damageAfter - damageBefore;

            if (damageApplied <= 0f)
                return null;

            NaniteDisruptionResult result =
                new NaniteDisruptionResult();

            result.IsMechanoid = false;

            result.Parts.Add(
                new NaniteDisruptionPartResult
                {
                    PartName = selected.parent.Label,
                    AppliedPercent = damageApplied * 100f,
                    TotalPercent = damageAfter * 100f
                });

            return result;
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

            HediffSet hediffSet = victim.health.hediffSet;

            List<BodyPartRecord> eligibleParts =
                new List<BodyPartRecord>();

            foreach (BodyPartRecord part
                in victim.RaceProps.body.AllParts)
            {
                if (hediffSet.PartIsMissing(part))
                    continue;

                Hediff_NaniteMechanicalDegradation existing =
                    FindDegradation(hediffSet, part);

                if (existing != null)
                {
                    HediffComp_NaniteDamage existingComp =
                        existing.TryGetComp<HediffComp_NaniteDamage>();

                    if (existingComp != null &&
                        existingComp.AccumulatedDamage >= MaxDamage)
                        continue;
                }

                eligibleParts.Add(part);
            }

            if (eligibleParts.Count == 0)
                return null;

            eligibleParts.Shuffle();

            int partsToDamage =
                eligibleParts.Count < 3
                    ? eligibleParts.Count
                    : 3;

            NaniteDisruptionResult result =
                new NaniteDisruptionResult();

            result.IsMechanoid = true;

            for (int i = 0; i < partsToDamage; i++)
            {
                BodyPartRecord part = eligibleParts[i];

                Hediff_NaniteMechanicalDegradation degradation =
                    FindDegradation(hediffSet, part);

                if (degradation == null)
                {
                    degradation =
                        HediffMaker.MakeHediff(
                            degradationDef,
                            victim,
                            part)
                        as Hediff_NaniteMechanicalDegradation;

                    if (degradation == null)
                        continue;

                    victim.health.AddHediff(
                        degradation,
                        part);
                }

                HediffComp_NaniteDamage comp =
                    degradation.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null)
                    continue;

                float damageBefore = comp.AccumulatedDamage;

                comp.ApplyDamage(effectStrength);

                float damageAfter = comp.AccumulatedDamage;
                float damageApplied =
                    damageAfter - damageBefore;

                if (damageApplied <= 0f)
                    continue;

                degradation.Severity = damageAfter;

                result.Parts.Add(
                    new NaniteDisruptionPartResult
                    {
                        PartName = part.Label,
                        AppliedPercent = damageApplied * 100f,
                        TotalPercent = damageAfter * 100f
                    });
            }

            if (result.Parts.Count == 0)
                return null;

            return result;
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

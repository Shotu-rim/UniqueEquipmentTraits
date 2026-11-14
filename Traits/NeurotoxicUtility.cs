using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace UniqueEquipmentTraits
{
    public static class NeurotoxicUtility
    {
        public sealed class CapacityTarget
        {
            public PawnCapacityDef Capacity;
            public List<BodyPartRecord> Parts;
        }

        public static List<CapacityTarget> GetAvailableTargets(Pawn pawn)
        {
            List<CapacityTarget> result = new List<CapacityTarget>();

            if (pawn == null || pawn.Dead || !pawn.RaceProps.IsFlesh)
                return result;

            BodyDef body = pawn.RaceProps.body;

            AddTarget(result, pawn, PawnCapacityDefOf.Hearing,
                BodyPartTagDefOf.HearingSource);

            AddTarget(result, pawn, PawnCapacityDefOf.Moving,
                BodyPartTagDefOf.MovingLimbCore,
                BodyPartTagDefOf.MovingLimbSegment,
                BodyPartTagDefOf.MovingLimbDigit);

            AddTarget(result, pawn, PawnCapacityDefOf.Manipulation,
                BodyPartTagDefOf.ManipulationLimbCore,
                BodyPartTagDefOf.ManipulationLimbSegment,
                BodyPartTagDefOf.ManipulationLimbDigit);

            AddTarget(result, pawn, PawnCapacityDefOf.Consciousness,
                BodyPartTagDefOf.ConsciousnessSource);

            AddTarget(result, pawn, PawnCapacityDefOf.Breathing,
                BodyPartTagDefOf.BreathingSource,
                BodyPartTagDefOf.BreathingPathway,
                BodyPartTagDefOf.BreathingSourceCage);

            AddTarget(result, pawn, PawnCapacityDefOf.Talking,
                BodyPartTagDefOf.TalkingSource,
                BodyPartTagDefOf.TalkingPathway,
                BodyPartTagDefOf.Tongue);

            AddTarget(result, pawn, PawnCapacityDefOf.BloodPumping,
                BodyPartTagDefOf.BloodPumpingSource);

            // BloodFiltration: Vanilla branch
            if (body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney))
            {
                AddTarget(result, pawn, PawnCapacityDefOf.BloodFiltration,
                    BodyPartTagDefOf.BloodFiltrationKidney,
                    BodyPartTagDefOf.BloodFiltrationLiver);
            }
            else
            {
                AddTarget(result, pawn, PawnCapacityDefOf.BloodFiltration,
                    BodyPartTagDefOf.BloodFiltrationSource);
            }

            // Metabolism: Def search
            PawnCapacityDef metabolism =
                DefDatabase<PawnCapacityDef>.GetNamedSilentFail("Metabolism");

            if (metabolism != null)
            {
                AddTarget(result, pawn, metabolism,
                    BodyPartTagDefOf.MetabolismSource);
            }

            return result;
        }

        private static void AddTarget(
            List<CapacityTarget> result,
            Pawn pawn,
            PawnCapacityDef capacity,
            params BodyPartTagDef[] tags)
        {
            if (capacity == null ||
                !capacity.Worker.CanHaveCapacity(pawn.RaceProps.body))
                return;

            List<BodyPartRecord> parts = new List<BodyPartRecord>();
            HashSet<BodyPartRecord> seen = new HashSet<BodyPartRecord>();

            foreach (BodyPartTagDef tag in tags)
            {
                foreach (BodyPartRecord part in
                    pawn.RaceProps.body.GetPartsWithTag(tag))
                {
                    if (pawn.health.hediffSet.PartIsMissing(part))
                        continue;

                    if (seen.Add(part))
                        parts.Add(part);
                }
            }

            if (parts.Count == 0)
                return;

            result.Add(new CapacityTarget
            {
                Capacity = capacity,
                Parts = parts
            });
        }



        public static void ApplyNeurotoxic(
            Pawn victim,
            float effectStrength,
            out int affectedCapacities,
            out float appliedWeakening)
        {
            affectedCapacities = 0;
            appliedWeakening = 0f;

            if (victim == null || victim.Dead || effectStrength <= 0f)
                return;

            List<CapacityTarget> targets = GetAvailableTargets(victim);

            if (targets.Count == 0)
                return;

            const float maxWeakeningPerCapacity = 0.60f;

            HediffDef neurotoxicDef =
                DefDatabase<HediffDef>.GetNamedSilentFail("UET_Neurotoxic");

            if (neurotoxicDef == null)
            {
                Log.Error("[UET] Missing HediffDef: UET_Neurotoxic");
                return;
            }

            // Overview counter, what's left to hit the limit
            List<CapacityTarget> eligible = new List<CapacityTarget>();
            List<float> available = new List<float>();

            foreach (CapacityTarget target in targets)
            {
                float current = 0f;

                foreach (Hediff hediff in victim.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Neurotoxic neurotoxic &&
                        neurotoxic.AffectedCapacity == target.Capacity)
                    {
                        current += neurotoxic.Weakening;
                    }
                }

                float free = maxWeakeningPerCapacity - current;

                if (free > 0.0001f)
                {
                    eligible.Add(target);
                    available.Add(free);
                }
            }

            if (eligible.Count == 0)
                return;

            float totalAvailable = 0f;

            foreach (float value in available)
                totalAvailable += value;

            float remainingBudget =
                UnityEngine.Mathf.Min(effectStrength, totalAvailable);


            // Weight counter
            List<float> weights = new List<float>();

            foreach (CapacityTarget target in eligible)
                weights.Add(UnityEngine.Random.Range(0.1f, 1f));

            // Distribute points by weight
            List<float> allocations = new List<float>();

            for (int i = 0; i < eligible.Count; i++)
                allocations.Add(0f);

            while (remainingBudget > 0.000001f)
            {
                float totalWeight = 0f;

                for (int i = 0; i < eligible.Count; i++)
                {
                    if (available[i] - allocations[i] > 0.000001f)
                        totalWeight += weights[i];
                }

                if (totalWeight <= 0f)
                    break;

                float distributed = 0f;

                for (int i = 0; i < eligible.Count; i++)
                {
                    float free = available[i] - allocations[i];

                    if (free <= 0.000001f)
                        continue;

                    float share = remainingBudget * weights[i] / totalWeight;
                    float amount = UnityEngine.Mathf.Min(share, free);

                    allocations[i] += amount;
                    distributed += amount;
                }

                if (distributed <= 0.000001f)
                    break;

                remainingBudget -= distributed;
            }

            // Set effects on body parts
            for (int i = 0; i < eligible.Count; i++)
            {
                float amount = allocations[i];

                if (amount <= 0.000001f)
                    continue;

                CapacityTarget target = eligible[i];
                BodyPartRecord part = target.Parts.RandomElement();

                Hediff_Neurotoxic existing = null;

                foreach (Hediff hediff in victim.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Neurotoxic neurotoxic &&
                        neurotoxic.AffectedCapacity == target.Capacity &&
                        neurotoxic.Part == part)
                    {
                        existing = neurotoxic;
                        break;
                    }
                }

                if (existing != null)
                {
                    existing.Severity += amount;
                    victim.health.Notify_HediffChanged(existing);
                }
                else
                {
                    Hediff_Neurotoxic newHediff =
                        (Hediff_Neurotoxic)HediffMaker.MakeHediff(
                            neurotoxicDef, victim, part);

                    newHediff.Initialize(target.Capacity, amount);
                    victim.health.AddHediff(newHediff, part);
                }

                // Count effect on affected parts
                affectedCapacities++;
                appliedWeakening += amount;
            }
        }



        public static void ProcessNaturalRecovery(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || !pawn.RaceProps.IsFlesh)
                return;

            List<Hediff_Neurotoxic> neurotoxins =
                new List<Hediff_Neurotoxic>();

            float totalToxicity = 0f;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_Neurotoxic neurotoxic &&
                    neurotoxic.Severity > 0f)
                {
                    neurotoxins.Add(neurotoxic);
                    totalToxicity += neurotoxic.Severity;
                }
            }

            if (neurotoxins.Count == 0)
                return;

            // Regen stats
            Hediff_Neurotoxic selected = neurotoxins.RandomElement();

            const float baseRecovery = 0.01f;
            const float minToxicityFactor = 0.25f;
            const float minFinalFactor = 0.10f;
            const float starvationMultiplier = 0.40f;

            // More toxin, less healing, up to 25% of max heal
            float toxicityFactor = UnityEngine.Mathf.Max(
                minToxicityFactor,
                1f / (1f + totalToxicity));

            float recoveryFactor = toxicityFactor;

            // Hunger reduces healing, but doesn't stop it
            if (pawn.needs?.food != null &&
                pawn.needs.food.Starving)
            {
                recoveryFactor *= starvationMultiplier;
            }

            // Rest and bed mods
            if (pawn.GetPosture() != PawnPosture.Standing)
            {
                recoveryFactor *= 1.5f;

                Building_Bed bed = pawn.CurrentBed();

                if (bed != null)
                {
                    recoveryFactor *=
                        1f + bed.def.building.bed_healPerDay / 10f;
                }
            }

            // Body healing capacity
            recoveryFactor *=
                UnityEngine.Mathf.Max(
                    0f,
                    pawn.GetStatValue(StatDefOf.InjuryHealingFactor));

            // 10% healing limit (to give a chance for rescue) even with hunger
            recoveryFactor = UnityEngine.Mathf.Max(
                minFinalFactor,
                recoveryFactor);

            float recovery = baseRecovery * recoveryFactor;

            selected.Severity -= recovery;

            if (selected.Severity <= 0f)
            {
                pawn.health.RemoveHediff(selected);
            }
            else
            {
                pawn.health.Notify_HediffChanged(selected);
            }
        }



        public static void ApplyGlitterworldTreatment(
            Pawn patient,
            float reduction)
        {
            if (patient == null || patient.Dead || reduction <= 0f)
                return;

            List<Hediff_Neurotoxic> neurotoxins =
                new List<Hediff_Neurotoxic>();

            foreach (Hediff hediff in patient.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_Neurotoxic neurotoxic &&
                    neurotoxic.Severity > 0f)
                {
                    neurotoxins.Add(neurotoxic);
                }
            }

            float remaining = reduction;

            while (remaining > 0.000001f && neurotoxins.Count > 0)
            {
                Hediff_Neurotoxic selected =
                    neurotoxins.RandomElement();

                neurotoxins.Remove(selected);

                float applied = UnityEngine.Mathf.Min(
                    selected.Severity,
                    remaining);

                remaining -= applied;

                if (selected.Severity <= applied)
                {
                    patient.health.RemoveHediff(selected);
                }
                else
                {
                    selected.Severity -= applied;
                    patient.health.Notify_HediffChanged(selected);
                }
            }
        }



    }



}

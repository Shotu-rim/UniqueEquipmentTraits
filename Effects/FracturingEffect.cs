
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class FracturingEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            Pawn victim = context.Victim;

            if (victim == null || victim.Dead || victim.health == null)
                return;

            HediffSet hediffSet = victim.health.hediffSet;

            List<BodyPartRecord> candidates =
                new List<BodyPartRecord>();

            foreach (KeyValuePair<BodyPartRecord, float> entry
                in context.PartDamage)
            {
                BodyPartRecord part = entry.Key;

                if (part == null || entry.Value <= 0f)
                    continue;

                if (!part.def.destroyableByDamage)
                    continue;

                if (hediffSet.PartIsMissing(part))
                    continue;

                if (hediffSet.GetPartHealth(part) <= 0f)
                    continue;

                if (hediffSet.HasDirectlyAddedPartFor(part))
                {
                    candidates.Add(part);
                    continue;
                }

                if (hediffSet
                    .PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                if (!IsBone(part))
                    continue;

                candidates.Add(part);
            }

            // Diagnostyka także wtedy, gdy nie znaleziono celu.
            if (candidates.Count == 0)
            {
                return;
            }


            BodyPartRecord target = candidates.RandomElement();

            bool isBionic =
                hediffSet.HasDirectlyAddedPartFor(target);

            string implantName = "unknown replacement";

            if (isBionic)
            {
                foreach (Hediff hediff in hediffSet.hediffs)
                {
                    if (hediff.Part == target &&
                        hediff is Hediff_AddedPart)
                    {
                        implantName = hediff.Label;
                        break;
                    }
                }

                DestroyAddedPart(victim, target);
            }
            else
            {
                DestroyBone(victim, target);
            }

            if (hediffSet.PartIsMissing(target))
            {
                if (isBionic)
                {
                    context.Report(
                        $"{context.Attacker.LabelShort} destroyed " +
                        $"{context.Victim.LabelShort}'s {implantName} " +
                        $"({target.Label}).");
                }
                else
                {
                    context.Report(
                        $"{context.Attacker.LabelShort} shattered " +
                        $"{context.Victim.LabelShort}'s {target.Label}.");
                }
            }

        }

        private static void DestroyBone(
            Pawn victim,
            BodyPartRecord target)
        {
            float remainingHealth =
                victim.health.hediffSet.GetPartHealth(target);

            Hediff_Injury injury =
                (Hediff_Injury)HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamed("Crack"),
                    victim,
                    target);

            injury.destroysBodyParts = true;
            injury.Severity = remainingHealth + 1f;

            victim.health.AddHediff(injury);
        }

        private static void DestroyAddedPart(
            Pawn victim,
            BodyPartRecord target)
        {
            Hediff_MissingPart missing =
                (Hediff_MissingPart)HediffMaker.MakeHediff(
                    HediffDefOf.MissingBodyPart,
                    victim,
                    target);

            missing.IsFresh = true;

            victim.health.AddHediff(missing);
        }

        private static bool IsBone(BodyPartRecord part)
        {
            switch (part.def.defName)
            {
                case "Skull":
                case "Ribcage":
                case "Spine":
                case "Humerus":
                case "Radius":
                case "Pelvis":
                case "Femur":
                case "Tibia":
                    return true;

                default:
                    return false;
            }
        }
    }
}

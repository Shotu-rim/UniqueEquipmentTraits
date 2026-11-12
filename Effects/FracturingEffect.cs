
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class FracturingEffect : UETEffect
    {
        private const float CrackCoefficient = 5f;
        private const float HealthReference = 30f;
        private const double HealthExponent = 0.75;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.PartDamage == null)
                return;

            Pawn victim = context.Victim;

            if (victim == null || victim.Dead || victim.health == null)
                return;

            HediffSet hediffSet = victim.health.hediffSet;

            if (hediffSet == null)
                return;

            HediffDef crackDef =
                DefDatabase<HediffDef>.GetNamedSilentFail("Crack");

            if (crackDef == null)
            {
                Log.Error("[UET] Fracturing: Crack HediffDef not found.");
                return;
            }

            // Zbieramy cele przed modyfikacją stanu zdrowia.
            List<KeyValuePair<BodyPartRecord, float>> bones =
                new List<KeyValuePair<BodyPartRecord, float>>();

            List<BodyPartRecord> replacements =
                new List<BodyPartRecord>();

            foreach (KeyValuePair<BodyPartRecord, float> entry
                in context.PartDamage)
            {
                BodyPartRecord part = entry.Key;
                float damage = entry.Value;

                if (part == null || damage <= 0f)
                    continue;

                if (!part.def.destroyableByDamage)
                    continue;

                if (hediffSet.PartIsMissing(part))
                    continue;

                if (hediffSet.GetPartHealth(part) <= 0f)
                    continue;

                if (hediffSet.HasDirectlyAddedPartFor(part))
                {
                    replacements.Add(part);
                    continue;
                }

                // Nie traktujemy części pod protezą
                // jako naturalnych kości.
                if (hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                if (!IsBone(part))
                    continue;

                bones.Add(entry);
            }

            // Jedna losowa trafiona część zastępcza.
            // Wybieramy ją przed nakładaniem Crack.
            BodyPartRecord replacementTarget = null;
            string replacementName = null;

            if (replacements.Count > 0)
            {
                replacementTarget = replacements.RandomElement();

                foreach (Hediff hediff in hediffSet.hediffs)
                {
                    if (hediff.Part == replacementTarget &&
                        hediff is Hediff_AddedPart)
                    {
                        replacementName = hediff.Label;
                        break;
                    }
                }
            }

            // Crack na każdej trafionej naturalnej kości.
            foreach (KeyValuePair<BodyPartRecord, float> entry in bones)
            {
                if (victim.Dead)
                    break;

                BodyPartRecord part = entry.Key;

                // Poprzednie obrażenia mogły zniszczyć
                // część nadrzędną.
                if (hediffSet.PartIsMissing(part))
                    continue;

                if (hediffSet.GetPartHealth(part) <= 0f)
                    continue;

                if (hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                float maxHealth = part.def.GetMaxHealth(victim);

                if (maxHealth <= 0f)
                    continue;

                float damage = entry.Value;

                float healthMultiplier = (float)Math.Pow(
                    maxHealth / HealthReference,
                    HealthExponent);

                float crackDamage =
                    CrackCoefficient *
                    (float)Math.Sqrt(damage) *
                    context.EffectStrength *
                    healthMultiplier;

                if (crackDamage <= 0f)
                    continue;

                Hediff_Injury injury = (Hediff_Injury)HediffMaker.MakeHediff(crackDef, victim, part);

                injury.Severity = crackDamage;
                injury.destroysBodyParts = true;

                // Oznaczamy obrażenie jako pochodzące od Fracturing.
                injury.sourceHediffDef =
                    DefDatabase<HediffDef>.GetNamed("UET_FracturingSource");

                victim.health.AddHediff(injury);

                if (hediffSet.PartIsMissing(part))
                {
                    context.Report(
                        $"{context.Attacker.LabelShort} shattered " +
                        $"{victim.LabelShort}'s {part.Label}.");
                }
                else
                {
                    context.Report(
                        $"{context.Attacker.LabelShort} fractured {victim.LabelShort}'s {part.Label}, dealing {crackDamage:F1} additional damage.");
                }
            }

            // Maksymalnie jedna część zastępcza.
            if (replacementTarget != null &&
                !victim.Dead &&
                !hediffSet.PartIsMissing(replacementTarget) &&
                hediffSet.HasDirectlyAddedPartFor(replacementTarget))
            {
                DestroyAddedPart(victim, replacementTarget);

                if (hediffSet.PartIsMissing(replacementTarget))
                {
                    context.Report(
                        $"{context.Attacker.LabelShort} destroyed " +
                        $"{victim.LabelShort}'s " +
                        $"{replacementName ?? "replacement"} " +
                        $"({replacementTarget.Label}).");
                }
            }
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

            missing.IsFresh = false;

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

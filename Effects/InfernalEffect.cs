
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class InfernalEffect : UETEffect
    {
        private const float BurnMultiplier = 10f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.health == null)
                return;

            // Infernal nie działa na mechanoidy.
            if (victim.RaceProps == null ||
                victim.RaceProps.IsMechanoid)
                return;

            float burnSeverity =
                context.EffectStrength * BurnMultiplier;

            if (burnSeverity <= 0f)
                return;

            HediffDef burnDef =
                DefDatabase<HediffDef>.GetNamedSilentFail("Burn");

            if (burnDef == null)
            {
                Log.Error("[UET] Missing HediffDef: Burn");
                return;
            }

            int burnsApplied = 0;
            List<string> burnedParts = new List<string>();

            foreach (BodyPartRecord part in context.HitParts)
            {
                if (part == null)
                    continue;

                // Nie nakładamy poparzeń na brakujące części.
                if (victim.health.hediffSet.PartIsMissing(part))
                    continue;

                Hediff_Injury burn =
                    HediffMaker.MakeHediff(
                        burnDef, victim, part) as Hediff_Injury;

                if (burn == null)
                    continue;

                burn.Severity = burnSeverity;

                victim.health.AddHediff(burn, part);

                burnsApplied++;
                burnedParts.Add(part.LabelCap);
            }

            if (burnsApplied > 0)
            {

                context.Report(
                    $"{context.Attacker.LabelShort} inflicted " +
                    $"{burnsApplied} {(burnsApplied == 1 ? "burn" : "burns")} " +
                    $"on {context.Victim.LabelShort}, each with a severity " +
                    $"of {burnSeverity:F1}, affecting: " +
                    $"{string.Join(", ", burnedParts)}.");

            }
        }
    }
}

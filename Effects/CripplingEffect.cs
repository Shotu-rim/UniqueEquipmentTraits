
using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class CripplingEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Victim == null ||
                context.Victim.health == null ||
                context.Victim.Dead)
                return;

            Pawn victim = context.Victim;

            float strength = context.EffectStrength;

            if (strength <= 0f)
                return;

            float bodyWeight =
                victim.GetStatValue(StatDefOf.Mass);

            if (bodyWeight <= 0f)
                return;

            double durationSeconds =
                50.0 *
                strength *
                strength *
                Math.Log(1.0 + bodyWeight);

            int durationTicks = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    (float)durationSeconds * 60f));

            HediffDef cripplingDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Crippling");

            if (cripplingDef == null)
            {
                Log.Error(
                    "[UET] Missing HediffDef: UET_Crippling");
                return;
            }

            Hediff_Crippling crippling =
                victim.health.hediffSet
                    .GetFirstHediffOfDef(cripplingDef)
                    as Hediff_Crippling;

            bool overwritten = crippling != null;

            if (crippling == null)
            {
                crippling = HediffMaker.MakeHediff(
                    cripplingDef, victim)
                    as Hediff_Crippling;

                if (crippling == null)
                    return;

                victim.health.AddHediff(crippling);
            }

            // Last activation matter, change both strength and time
            crippling.ApplyCrippling(
                strength,
                durationTicks);

            context.Report(
                $"{context.Attacker.LabelShort} slowed " +
                $"{context.Victim.LabelShort}, reducing their movement speed " +
                $"by {strength:P0} for {durationSeconds:0.#} seconds " +
                $"(target mass: {bodyWeight:0.#} kg). " +
                (overwritten
                ? "The previous effect was replaced."
                : ""));
        }
    }
}

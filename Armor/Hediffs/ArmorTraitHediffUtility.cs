
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Manages hediffs originating from individual
    /// armor items and their traits.
    /// </summary>
    public static class ArmorTraitHediffUtility
    {
        // Applies hediff

        public static Hediff Apply(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait,
            HediffDef hediffDef)
        {
            if (pawn == null ||
                pawn.health == null ||
                apparel == null ||
                trait == null ||
                hediffDef == null)
            {
                return null;
            }

            // Check whether this exact item and trait
            // already provide the requested hediff.
            foreach (Hediff existing in
                pawn.health.hediffSet.hediffs)
            {
                if (existing.def != hediffDef)
                    continue;

                HediffComp_ArmorTraitSource existingSource =
                    existing.TryGetComp<HediffComp_ArmorTraitSource>();

                if (existingSource != null &&
                    existingSource.IsFrom(apparel, trait))
                {
                    return existing;
                }
            }

            Hediff hediff = HediffMaker.MakeHediff(
                hediffDef,
                pawn);

            HediffComp_ArmorTraitSource source =
                hediff.TryGetComp<HediffComp_ArmorTraitSource>();

            if (source == null)
            {
                Log.Error(
                    "[UET Armor] Hediff " +
                    hediffDef.defName +
                    " is missing HediffComp_ArmorTraitSource.");

                return null;
            }

            source.SetSource(apparel, trait);

            pawn.health.AddHediff(hediff);

            return hediff;
        }


        // Removes all hediffs
        public static void Remove(
    Pawn pawn,
    Apparel apparel,
    ArmorTraitDef trait)
        {
            if (pawn == null ||
                pawn.health == null ||
                apparel == null ||
                trait == null)
            {
                return;
            }

            List<Hediff> toRemove = new List<Hediff>();

            foreach (Hediff hediff in
                pawn.health.hediffSet.hediffs)
            {
                HediffComp_ArmorTraitSource source =
                    hediff.TryGetComp<HediffComp_ArmorTraitSource>();

                if (source != null &&
                    source.IsFrom(apparel, trait))
                {
                    toRemove.Add(hediff);
                }
            }

            foreach (Hediff hediff in toRemove)
            {
                if (pawn.health.hediffSet.hediffs.Contains(hediff))
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        // Removes all hediffs
        public static void RemoveAllFromApparel(
            Pawn pawn,
            Apparel apparel)
        {
            if (pawn == null ||
                pawn.health == null ||
                apparel == null)
            {
                return;
            }

            var hediffs = pawn.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];

                HediffComp_ArmorTraitSource source =
                    hediff.TryGetComp<HediffComp_ArmorTraitSource>();

                if (source == null)
                    continue;

                if (source.SourceApparel != apparel)
                    continue;

                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}

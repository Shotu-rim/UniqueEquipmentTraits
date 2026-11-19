
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Effects;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Collects active weapon effects for the Effects panel.
    /// Reuses existing Hediffs to preserve their original
    /// labels, colors, tooltips, and information cards.
    /// </summary>
    public static class UETWeaponEffectDisplayAdapter
    {
        private const string FlurryHediffDefName =
            "UET_FlurryActive";

        private const string TailwindHediffDefName =
            "UET_TailwindActive";

        public static void Collect(
            Pawn pawn,
            List<UETEffectDisplayEntry> entries)
        {
            if (pawn == null || entries == null ||
                pawn.health == null ||
                pawn.equipment == null)
                return;

            ThingWithComps weapon = pawn.equipment.Primary;

            if (weapon == null)
                return;

            CompWeaponTrait comp =
                weapon.TryGetComp<CompWeaponTrait>();

            if (comp == null)
                return;

            if (comp.FlurryActiveFor(pawn))
            {
                AddHediffEffect(
                    pawn,
                    weapon,
                    entries,
                    FlurryHediffDefName,
                    comp.FlurryCharges);
            }

            if (comp.TailwindActiveFor(pawn))
            {
                AddHediffEffect(
                    pawn,
                    weapon,
                    entries,
                    TailwindHediffDefName,
                    comp.TailwindCharges);
            }
        }

        private static void AddHediffEffect(
            Pawn pawn,
            ThingWithComps weapon,
            List<UETEffectDisplayEntry> entries,
            string hediffDefName,
            int charges)
        {
            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    hediffDefName);
            CompQuality quality = weapon.TryGetComp<CompQuality>();

            int maxCharges = 0;

            if (quality != null)
            {
                if (hediffDefName == FlurryHediffDefName)
                    maxCharges = FlurryEffect.InitialCharges(quality.Quality);
                else if (hediffDefName == TailwindHediffDefName)
                    maxCharges = TailwindEffect.InitialCharges(quality.Quality);
            }
            if (def == null)
                return;

            Hediff hediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(def);

            if (hediff == null)
                return;

            entries.Add(new UETEffectDisplayEntry(
                weapon,
                hediffDefName,
                hediff.LabelCap,
                hediff.TipStringExtra,
                charges,
                maxCharges,
                true,
                hediff));
        }
    }
}

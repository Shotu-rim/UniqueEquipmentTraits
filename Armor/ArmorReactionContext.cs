
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Tracks reactive armor candidates during melee attacks.
    /// Observes hit body parts, prevents armor reuse, and processes
    /// up to three reaction stacks with independent activation chances.
    /// UET extensions can supply hit events through ObservePart().
    /// </summary>
    internal sealed class ArmorReactionContext
    {
        internal readonly Pawn Attacker;
        internal readonly Pawn Defender;

        internal readonly HashSet<Apparel> Participated =
            new HashSet<Apparel>();

        internal readonly List<BodyPartRecord> ObservedParts =
            new List<BodyPartRecord>();

        internal int EligibleStacks;

        internal ArmorReactionContext(Pawn attacker, Pawn defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        internal void ObservePart(
            BodyPartRecord part,
            DamageInfo dinfo,
            float injurySeverity)
        {
            if (part == null || Defender == null)
                return;

            ObservedParts.Add(part);

            if (Defender.apparel == null)
                return;

            List<Apparel> candidates = new List<Apparel>();

            foreach (Apparel apparel in Defender.apparel.WornApparel)
            {
                if (apparel == null ||
                    !ArmorTraitCoverage.CoversBodyPart(apparel.def, part))
                {
                    continue;
                }

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                // Only armor with reactive traits participates in random
                if (comp == null || !comp.HasReactiveTrait)
                    continue;

                if (!Participated.Contains(apparel))
                    candidates.Add(apparel);
            }

            if (EligibleStacks >= 3)
                return;

            if (candidates.Count == 0)
                return;

            EligibleStacks++;

            // Exclude participating armor from subsequent stacks, not just the selected one
            foreach (Apparel apparel in candidates)
                Participated.Add(apparel);

            Apparel selectedApparel;
            ArmorTraitDef selectedTrait;

            bool selected = ArmorReactionSelection.TrySelect(
                candidates,
                out selectedApparel,
                out selectedTrait);

            if (!selected)
                return;

            if (!ArmorReactionActivation.TryActivate(EligibleStacks))
                return;

            ArmorTraitActivationContext activationContext =
                new ArmorTraitActivationContext(
                    Attacker,
                    Defender,
                    selectedApparel,
                    selectedTrait,
                    part,
                    EligibleStacks,
                    dinfo,
                    injurySeverity);

            selectedTrait.Worker.OnActivated(activationContext);
        }
    }
}


using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Tracks reactive armor candidates during melee attacks.
    /// Observes hit body parts, prevents armor reuse, and processes
    /// up to three non-empty reaction stacks with independent
    /// activation chances.
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

        internal ArmorReactionContext(
            Pawn attacker,
            Pawn defender)
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

            if (EligibleStacks >= 3)
                return;

            List<Apparel> candidates =
                new List<Apparel>();

            foreach (Apparel apparel in Defender.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                if (!ArmorTraitCoverage.CoversBodyPart(
                    apparel.def,
                    part))
                {
                    continue;
                }

                if (Participated.Contains(apparel))
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    !comp.CanActivateReactive())
                {
                    continue;
                }

                candidates.Add(apparel);
            }

            // Empty candidate lists never consume a stack
            if (candidates.Count == 0)
                return;

            Apparel selectedApparel;
            ArmorTraitDef selectedTrait;

            // Select from ready candidates before consuming a stack
            if (!ArmorReactionSelection.TrySelect(
                candidates,
                out selectedApparel,
                out selectedTrait))
            {
                return;
            }

            // This is now a valid, non-empty reaction stack
            EligibleStacks++;

            // Every candidate participates, even when activation roll fails
            foreach (Apparel apparel in candidates)
            {
                Participated.Add(apparel);
            }

            if (!ArmorReactionActivation.TryActivate(
                EligibleStacks))
            {
                return;
            }

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


            ArmorTraitWorker worker = selectedTrait.Worker;

            if (!worker.CanActivate(activationContext))
                return;

            CompArmorTraits selectedComp =
                selectedApparel.TryGetComp<CompArmorTraits>();

            if (selectedComp == null ||
                !selectedComp.CanActivateReactive())
            {
                return;
            }

            // Consume one charge after activation

            if (!selectedComp.State.TryConsumeCharge())
                return;

            worker.OnActivated(activationContext);

                selectedComp.State.StartActivation(
                selectedTrait.activeEffectTicks,
                selectedTrait.cooldownTicks);
                selectedComp.State.SetLastTriggeredPart(part);

        }
    }
}

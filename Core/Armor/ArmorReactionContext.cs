using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    // Observation-only. Completely independent of Weapon Traits.
    internal sealed class ArmorReactionContext
    {
        internal readonly Pawn Attacker;
        internal readonly Pawn Defender;
        internal readonly HashSet<Apparel> Participated = new HashSet<Apparel>();
        internal readonly List<BodyPartRecord> ObservedParts = new List<BodyPartRecord>();
        internal int EligibleStacks;
        private int eventNumber;

        internal ArmorReactionContext(Pawn attacker, Pawn defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        internal void ObservePart(BodyPartRecord part, DamageInfo dinfo, float injurySeverity)
        {
            if (part == null || Defender == null) return;
            ObservedParts.Add(part);
            if (Defender.apparel == null) return;
            int eventId = ++eventNumber;
            bool verbose = ArmorReactionDiagnostics.Enabled;
            if (verbose)
                Log.Message("[UET ARMOR STAGE2] hit part event=" + eventId +
                    " part=" + part.Label + " attacker=" + PawnName(Attacker) +
                    " defender=" + PawnName(Defender));

            List<Apparel> candidates = new List<Apparel>();
            List<Apparel> excluded = new List<Apparel>();

            foreach (Apparel apparel in Defender.apparel.WornApparel)
            {
                if (apparel == null ||
                    !ArmorTraitCoverage.CoversBodyPart(apparel.def, part))
                {
                    continue;
                }

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                // Bez reaktywnego traitu pancerz nie bierze
                // udziału w losowaniu ani nie jest wykluczany.
                if (comp == null || !comp.HasReactiveTrait)
                    continue;

                if (Participated.Contains(apparel))
                    excluded.Add(apparel);
                else
                    candidates.Add(apparel);
            }


            if (verbose)
            {
                Log.Message("[UET ARMOR STAGE2]   fresh=[" + Format(candidates) +
                    "] previously_used=[" + Format(excluded) + "]");
            }
            if (EligibleStacks >= 3)
            {
                if (verbose) Log.Message("[UET ARMOR STAGE2]   SKIP: three candidate stacks already processed");
                return;
            }
            if (candidates.Count == 0)
            {
                if (verbose) Log.Message("[UET ARMOR STAGE2]   SKIP: no unused covering apparel; attempt not consumed");
                return;
            }


            EligibleStacks++;

            // Every participating item is excluded,
            // not just the randomly selected winner.
            foreach (Apparel apparel in candidates)
                Participated.Add(apparel);

            Apparel selectedApparel;
            ArmorTraitDef selectedTrait;

            bool selected = ArmorReactionSelection.TrySelect(
                candidates,
                out selectedApparel,
                out selectedTrait);



            bool activated = false;

            if (selected)
            {
                activated = ArmorReactionActivation.TryActivate(
                    EligibleStacks);

                if (activated)
                {
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

                    selectedTrait.Worker.OnActivated(
                        activationContext);
                }
            }



            if (verbose)
            {
                Log.Message(
                    "[UET ARMOR STAGE2]   CANDIDATE STACK #" +
                    EligibleStacks +
                    " participants=[" + Format(candidates) +
                    "] -> ALL excluded from later stacks; observed parts=" +
                    ObservedParts.Count);

                if (selected)
                {
                    Log.Message(
                        "[UET ARMOR STAGE4]   SELECTED stack=" +
                        EligibleStacks +
                        " apparel=" + selectedApparel.def.defName +
                        "#" + selectedApparel.thingIDNumber +
                        " trait=" + selectedTrait.defName);


                    Log.Message(
                        "[UET ARMOR STAGE4C]   ACTIVATION stack=" +
                        EligibleStacks +
                        " chance=" +
                        (ArmorReactionActivation.GetActivationChance(
                            EligibleStacks) * 100f).ToString("F0") +
                        "%" +
                        " result=" +
                        (activated ? "SUCCESS" : "FAILED") +
                        " apparel=" +
                        selectedApparel.def.defName +
                        " trait=" +
                        selectedTrait.defName);

                }
                else
                {
                    Log.Warning(
                        "[UET ARMOR STAGE4] Selection failed for stack " +
                        EligibleStacks);

                }
            }

        }

        private static string PawnName(Pawn pawn)
        {
            return pawn == null ? "null" : pawn.LabelShort;
        }

        private static string Format(List<Apparel> apparelList)
        {
            if (apparelList.Count == 0) return "none";
            List<string> names = new List<string>();
            foreach (Apparel apparel in apparelList)
                names.Add(apparel.def.defName + "#" + apparel.thingIDNumber);
            return string.Join(", ", names.ToArray());
        }
    }

}

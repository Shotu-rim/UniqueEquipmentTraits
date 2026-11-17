
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Randomly selects an eligible armor item and one of its reactive traits.
    /// Filters out armor without reactive traits before selection.
    /// UET extensions can use TrySelect() to reuse this selection mechanism.
    /// </summary>
    internal static class ArmorReactionSelection
    {
        internal static bool TrySelect(
            List<Apparel> candidates,
            out Apparel selectedApparel,
            out ArmorTraitDef selectedTrait)
        {
            selectedApparel = null;
            selectedTrait = null;

            if (candidates == null || candidates.Count == 0)
                return false;

            List<Apparel> eligible = new List<Apparel>();

            foreach (Apparel apparel in candidates)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp != null && comp.HasReactiveTrait)
                    eligible.Add(apparel);
            }

            if (eligible.Count == 0)
                return false;

            selectedApparel = eligible.RandomElement();

            CompArmorTraits selectedComp =
                selectedApparel.TryGetComp<CompArmorTraits>();

            List<ArmorTraitDef> reactiveTraits =
                new List<ArmorTraitDef>();

            foreach (ArmorTraitDef trait in selectedComp.Traits)
            {

                if (trait != null &&
                    trait.activationType == ArmorTraitActivationType.Reactive)
                {
                    reactiveTraits.Add(trait);
                }

            }

            if (reactiveTraits.Count == 0)
            {
                selectedApparel = null;
                return false;
            }

            selectedTrait = reactiveTraits.RandomElement();
            return true;
        }
    }
}

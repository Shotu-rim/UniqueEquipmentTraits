
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
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
                if (trait != null && trait.reactive)
                    reactiveTraits.Add(trait);
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

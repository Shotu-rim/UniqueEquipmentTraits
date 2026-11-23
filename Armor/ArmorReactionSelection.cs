
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Randomly selects an eligible apparel item
    /// with a ready reactive armor trait.
    /// </summary>
    internal static class ArmorReactionSelection
    {
        internal static bool TrySelect(
            List<Apparel> candidates,
            Pawn attacker,
            out Apparel selectedApparel,
            out ArmorTraitDef selectedTrait)
        {
            selectedApparel = null;
            selectedTrait = null;

            if (candidates == null || candidates.Count == 0)
                return false;

            List<Apparel> eligible =
                new List<Apparel>();

            foreach (Apparel apparel in candidates)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    !comp.CanActivateReactive())
                {
                    continue;
                }

                bool hasEligibleReactive = false;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.activationType !=
                        ArmorTraitActivationType.Reactive)
                    {
                        continue;
                    }

                    if (trait.defName == "UET_Shock" &&
                        !UETShockUtility.CanShock(attacker))
                    {
                        continue;
                    }
                    // Damage triggered traits are handled separately in ArmorReactionDamageProcessor
                    if (trait.defName == "UET_Hemocatalyst" ||
                        trait.defName == "UET_ThermalVent")
                    {
                        continue;
                    }
                    hasEligibleReactive = true;
                    break;
                }

                if (hasEligibleReactive)
                    eligible.Add(apparel);
            }

            if (eligible.Count == 0)
                return false;

            selectedApparel = eligible.RandomElement();

            CompArmorTraits selectedComp =
                selectedApparel.TryGetComp<CompArmorTraits>();

            foreach (ArmorTraitDef trait in selectedComp.Traits)
            {
                if (trait == null ||
                    trait.activationType !=
                    ArmorTraitActivationType.Reactive)
                {
                    continue;
                }

                if (trait.defName == "UET_Shock" &&
                    !UETShockUtility.CanShock(attacker))
                {
                    continue;
                }

                selectedTrait = trait;
                return true;
            }

            selectedApparel = null;
            return false;
        }
    }
}

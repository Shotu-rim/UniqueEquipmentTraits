
using System.Collections.Generic;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Collects armor trait information for the Effects panel.
    /// Reads persistent state from individual apparel items.
    /// </summary>
    public static class UETArmorEffectDisplayAdapter
    {
        public static void Collect(
            Pawn pawn,
            List<UETEffectDisplayEntry> entries)
        {
            if (pawn == null ||
                entries == null ||
                pawn.apparel == null)
            {
                return;
            }

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    comp.Traits == null ||
                    comp.Traits.Count == 0 ||
                    comp.State == null)
                {
                    continue;
                }

                ArmorTraitDef trait = comp.Traits[0];

                if (trait == null)
                    continue;

                ArmorTraitState state = comp.State;
                bool isPassive =
                    trait.activationType == ArmorTraitActivationType.Passive;

                state.UpdateChargeRegeneration();

                entries.Add(new UETEffectDisplayEntry(
                    apparel,
                    trait.defName,
                    trait.LabelCap,
                    trait.description ?? string.Empty,
                    state.Charges,
                    state.MaxCharges,
                    true,
                    null,
                    state.RemainingActiveTicks,
                    state.RemainingCooldownTicks,
                    state.RemainingChargeTicks,
                    state.LastTriggeredPartLabel,
                    isPassive));
            }
        }
    }
}


using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Manages equipment-related UET state and effect cleanup.
    /// Supports both weapon and armor trait lifecycle events.
    /// </summary>
    public static class UETEquipmentStateManager
    {
        // WEAPON TRAITS

        public static void NotifyEquipmentRemoved(
            Pawn pawn,
            ThingWithComps equipment)
        {
            if (pawn == null || equipment == null)
                return;

            CompWeaponTrait comp =
                equipment.TryGetComp<CompWeaponTrait>();

            if (comp == null)
                return;

            comp.ClearFlurry();
            comp.ClearTailwind();
        }

        // ARMOR TRAITS

        public static void NotifyApparelAdded(
            Pawn pawn,
            Apparel apparel)
        {
            if (pawn == null || apparel == null)
                return;

            CompArmorTraits comp =
                apparel.TryGetComp<CompArmorTraits>();

            if (comp == null || comp.Traits == null)
                return;

            foreach (ArmorTraitDef trait in comp.Traits)
            {
                if (trait == null)
                    continue;

                trait.Worker.OnEquipped(
                    pawn,
                    apparel,
                    trait);
            }
        }

        public static void NotifyApparelRemoved(
            Pawn pawn,
            Apparel apparel)
        {
            if (pawn == null || apparel == null)
                return;

            // Remove all hediffs originating from this item.
            ArmorTraitHediffUtility.RemoveAllFromApparel(
                pawn,
                apparel);

            CompArmorTraits comp =
                apparel.TryGetComp<CompArmorTraits>();

            if (comp == null || comp.Traits == null)
                return;

            foreach (ArmorTraitDef trait in comp.Traits)
            {
                if (trait == null)
                    continue;

                trait.Worker.OnUnequipped(
                    pawn,
                    apparel,
                    trait);
            }
        }

        public static void NotifyWearerDied(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                ArmorTraitHediffUtility.RemoveAllFromApparel(
                    pawn,
                    apparel);

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null || comp.Traits == null)
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null)
                        continue;

                    trait.Worker.OnWearerDied(
                        pawn,
                        apparel,
                        trait);
                }
            }
        }
        public static void CheckExpiredArmorEffects(Pawn pawn)
        {
            if (pawn == null ||
                pawn.health == null ||
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
                    comp.State == null ||
                    comp.Traits == null)
                {
                    continue;
                }

                ArmorTraitState state = comp.State;

                if (state.ActiveEffectEndTick < 0 ||
                    state.IsEffectActive)
                {
                    continue;
                }

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.activationType !=
                        ArmorTraitActivationType.Reactive)
                    {
                        continue;
                    }

                    ArmorTraitHediffUtility.Remove(
                        pawn,
                        apparel,
                        trait);
                }
            }
        }
    }
}

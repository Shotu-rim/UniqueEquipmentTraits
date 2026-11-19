
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Base class for custom armor trait effect workers.
    /// Handles activation and equipment lifecycle events.
    /// </summary>
    public abstract class ArmorTraitWorker
    {
        // Checks whether trait can be activated
        public virtual bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return true;
        }

        // Executes trait
        public virtual void OnActivated(
            ArmorTraitActivationContext context)
        {
        }


        public virtual void OnEquipped(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait)
        {

        }

        public virtual void OnUnequipped(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait)
        {

        }

        public virtual void OnWearerDied(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait)
        {


            OnUnequipped(pawn, apparel, trait);
        }

    }
}

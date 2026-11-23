
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
        // Additional activation chance, independent of reaction stacks
        public virtual float GetActivationChance(
            ArmorTraitActivationContext context)
        {
            return 1f;
        }

        // Cooldown duration for the current equipment quality
        public virtual int GetCooldownTicks(
            ArmorTraitActivationContext context)
        {
            return context.Trait.cooldownTicks;
        }
        public virtual bool TryActivate(
            ArmorTraitActivationContext context)
        {
            if (!CanActivate(context))
                return false;

            OnActivated(context);
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

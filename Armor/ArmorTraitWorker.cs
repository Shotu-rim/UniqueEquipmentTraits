namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Base class for custom armor trait effect workers.
    /// Provides an overridable activation hook for reactive armor traits.
    /// Other mods can inherit from this class and override OnActivated()
    /// to implement their own armor trait effects.
    /// </summary>
    public abstract class ArmorTraitWorker
    {
        public virtual void OnActivated(
            ArmorTraitActivationContext context)
        {
        }
    }
}


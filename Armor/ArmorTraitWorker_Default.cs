
namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Provides a no-op fallback worker for armor traits without a custom worker.
    /// Allows traits to activate safely without applying additional effects.
    /// Other mods can use this worker for traits that require no activation behavior.
    /// </summary>
    public class ArmorTraitWorker_Default : ArmorTraitWorker
    {
        public override void OnActivated(
            ArmorTraitActivationContext context)
        {
        }
    }
}

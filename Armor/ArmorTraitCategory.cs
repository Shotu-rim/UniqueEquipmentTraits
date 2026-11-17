
namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Defines anatomical categories used to classify armor trait coverage.
    /// Other mods can reference these values when assigning or filtering
    /// armor traits for specific body regions.
    /// </summary>
    public enum ArmorTraitCategory
    {
        None = 0,

        Helmet = 1,
        Neck = 2,
        Shoulders = 3,
        Torso = 4,
        Arms = 5,
        Hands = 6,
        Waist = 7,
        Legs = 8,
        Feet = 9
    }
}

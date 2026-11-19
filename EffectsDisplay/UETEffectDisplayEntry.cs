
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Represents one equipment effect displayed to the player.
    /// Supports Hediff-based and equipment-based effects.
    /// </summary>
    public class UETEffectDisplayEntry
    {
        public Thing Source { get; private set; }
        public Hediff Hediff { get; private set; }
        public string BodyPartLabel { get; private set; }
        public bool IsPassive { get; private set; }

        public string EffectId { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }

        public int Charges { get; private set; }
        public int MaxCharges { get; private set; }
        public bool IsActive { get; private set; }

        public int RemainingActiveTicks { get; private set; }
        public int RemainingCooldownTicks { get; private set; }
        public int RemainingChargeTicks { get; private set; }

        public UETEffectDisplayEntry(
            Thing source,
            string effectId,
            string label,
            string description,
            int charges,
            int maxCharges,
            bool isActive,
            Hediff hediff = null,
            int remainingActiveTicks = 0,
            int remainingCooldownTicks = 0,
            int remainingChargeTicks = 0,
            string bodyPartLabel = null,
            bool isPassive = false)
        {
            Source = source;
            Hediff = hediff;

            EffectId = effectId;
            Label = label;
            Description = description;

            Charges = charges;
            MaxCharges = maxCharges;
            IsActive = isActive;

            RemainingActiveTicks = remainingActiveTicks;
            RemainingCooldownTicks = remainingCooldownTicks;
            RemainingChargeTicks = remainingChargeTicks;
            BodyPartLabel = bodyPartLabel;
            IsPassive = isPassive;
        }
    }
}

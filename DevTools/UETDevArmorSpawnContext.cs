
using RimWorld;
using System;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Maintains a thread-local context for spawning unique UET armor
    /// with a developer-selected trait and quality.
    /// Validates spawn parameters, identifies the selected armor variant,
    /// and exposes the selection during item initialization.
    /// Used internally by UET developer tools and armor generation patches.
    /// </summary>
    internal static class UETDevArmorSpawnContext
    {
        [ThreadStatic]
        private static ThingDef selectedArmorDef;

        [ThreadStatic]
        private static ArmorTraitDef selectedTrait;

        [ThreadStatic]
        private static QualityCategory? selectedQuality;

        internal static bool IsActive =>
            selectedArmorDef != null &&
            selectedTrait != null &&
            selectedQuality.HasValue;

        internal static ArmorTraitDef SelectedTrait =>
            selectedTrait;

        internal static QualityCategory SelectedQuality =>
            selectedQuality.GetValueOrDefault();

        internal static void Begin(
            ThingDef armorDef,
            ArmorTraitDef trait,
            QualityCategory quality)
        {
            if (armorDef == null)
                throw new ArgumentNullException(nameof(armorDef));

            if (trait == null)
                throw new ArgumentNullException(nameof(trait));

            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quality));
            }

            if (IsActive)
            {
                throw new InvalidOperationException(
                    "UET armor spawn context is already active.");
            }

            selectedArmorDef = armorDef;
            selectedTrait = trait;
            selectedQuality = quality;
        }

        internal static bool Matches(ThingWithComps armor)
        {
            return IsActive &&
                   armor != null &&
                   armor.def == selectedArmorDef;
        }

        internal static void End()
        {
            selectedArmorDef = null;
            selectedTrait = null;
            selectedQuality = null;
        }
    }
}

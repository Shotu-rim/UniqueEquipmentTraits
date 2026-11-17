
using RimWorld;
using System;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Maintains a thread-local context for spawning unique UET weapons
    /// with a developer-selected trait and quality.
    /// Validates spawn parameters and exposes the selected weapon,
    /// trait, and quality during item initialization.
    /// Used internally by UET developer tools and weapon generation patches.
    /// </summary>
    internal static class UETDevSpawnContext
    {
        [ThreadStatic]
        private static WeaponTraitDef selectedTrait;

        [ThreadStatic]
        private static QualityCategory? selectedQuality;

        [ThreadStatic]
        private static ThingDef selectedWeaponDef;

        internal static bool IsActive =>
            selectedTrait != null &&
            selectedQuality.HasValue &&
            selectedWeaponDef != null;

        internal static WeaponTraitDef SelectedTrait =>
            selectedTrait;

        internal static QualityCategory SelectedQuality =>
            selectedQuality.GetValueOrDefault();

        internal static void Begin(
            ThingDef weaponDef,
            WeaponTraitDef trait,
            QualityCategory quality)
        {
            if (weaponDef == null)
                throw new ArgumentNullException(nameof(weaponDef));

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
                    "UET dev spawn context is already active.");
            }

            selectedWeaponDef = weaponDef;
            selectedTrait = trait;
            selectedQuality = quality;
        }

        internal static bool Matches(ThingWithComps weapon)
        {
            return IsActive &&
                   weapon != null &&
                   weapon.def == selectedWeaponDef;
        }

        internal static void End()
        {
            selectedWeaponDef = null;
            selectedTrait = null;
            selectedQuality = null;
        }
    }
}

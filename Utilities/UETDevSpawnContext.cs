
using System;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
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

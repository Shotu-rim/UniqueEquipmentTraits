
using RimWorld;
using System;
using System.Linq;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using UniqueEquipmentTraits.Utilities;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    internal static class UETDevWeaponSpawner
    {
        internal static ThingWithComps Create(
            ThingDef originalDef,
            ThingDef stuffDef,
            RimWorld.WeaponTraitDef trait,
            QualityCategory quality)
        {
            if (!Prefs.DevMode)
            {
                Log.Warning(
                    "[UET] Dev weapon spawner requires dev mode.");
                return null;
            }

            if (originalDef == null || trait == null)
                return null;

            if (!originalDef.IsWeapon ||
                !originalDef.HasComp<CompQuality>())
                return null;

            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return null;

            // Only UET traits can be selected
            if (trait.GetModExtension<UETTraitExtension>() == null)
                return null;

            ThingDef spawnDef = originalDef;

            // Use generated UET variant if available
            if (UniqueWeaponVariantRegistry.TryGetVariant(
                    originalDef, out ThingDef variant))
            {
                spawnDef = variant;
            }

            // Unique weapon component required for Odyssey trait pipeline
            if (!spawnDef.HasComp<CompUniqueWeapon>())
            {
                Log.Warning(
                    "[UET] No unique weapon variant for " +
                    originalDef.defName);
                return null;
            }

            if (spawnDef.MadeFromStuff)
            {
                if (stuffDef == null ||
                    !stuffDef.IsStuff ||
                    !spawnDef.stuffCategories
                        .Any(category =>
                            stuffDef.stuffProps.categories
                                .Contains(category)))
                {
                    Log.Warning(
                        "[UET] Invalid material for " +
                        originalDef.defName);
                    return null;
                }
            }
            else
            {
                stuffDef = null;
            }

            // Check whether selected trait belongs to a category supported by this unique weapon
            CompProperties_UniqueWeapon uniqueProps =
                spawnDef.comps
                    .OfType<CompProperties_UniqueWeapon>()
                    .FirstOrDefault();

            if (uniqueProps == null ||
                uniqueProps.weaponCategories == null ||
                trait.weaponCategory == null ||
                !uniqueProps.weaponCategories.Contains(trait.weaponCategory) ||
                !trait.canGenerateAlone)
            {
                Log.Warning(
                    "[UET] Incompatible dev trait " +
                    trait.defName + " for " + spawnDef.defName);

                return null;
            }

            ThingWithComps weapon = null;

            UETDevSpawnContext.Begin(
                spawnDef, trait, quality);

            try
            {
                weapon = ThingMaker.MakeThing(
                    spawnDef, stuffDef) as ThingWithComps;
            }
            finally
            {
                UETDevSpawnContext.End();
            }

            if (weapon == null)
                return null;

            CompQuality compQuality =
                weapon.TryGetComp<CompQuality>();

            compQuality?.SetQuality(
                quality,
                ArtGenerationContext.Outsider);

            CompUniqueWeapon uniqueComp =
                weapon.TryGetComp<CompUniqueWeapon>();

            if (uniqueComp == null ||
                !uniqueComp.TraitsListForReading.Contains(trait))
            {
                Log.Error(
                    "[UET] Failed to initialize selected trait: " +
                    trait.defName);

                weapon.Destroy();
                return null;
            }

            return weapon;
        }
    }
}


using RimWorld;
using System;
using System.Linq;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Creates unique UET armor for developer testing.
    /// Validates trait compatibility, quality, materials, and variant
    /// availability before spawning an item with the selected trait.
    /// Uses the developer spawn context to control initialization
    /// and reports creation results through the debug log.
    /// Intended for internal development tools only.
    /// </summary>
    internal static class UETDevArmorSpawner
    {
        internal static ThingWithComps Create(
            ThingDef apparelDef,
            ThingDef stuffDef,
            ArmorTraitDef trait,
            QualityCategory quality)
        {
            if (!Prefs.DevMode)
                return null;

            if (apparelDef == null || trait == null)
                return null;

            if (!ArmorTraitCompatibility.CanApply(
                    apparelDef, trait))
            {
                Log.Warning(
                    "[UET ARMOR UNIQUE TEST] Incompatible trait: " +
                    trait.defName + " for " + apparelDef.defName);

                return null;
            }

            if (quality != QualityCategory.Masterwork &&
                quality != QualityCategory.Legendary)
                return null;

            ThingDef originalDef = apparelDef;

            ThingDef variantDef;

            if (!UniqueArmorVariantRegistry.TryGetVariant(
                    originalDef, out variantDef))
            {
                Log.Warning(
                    "[UET ARMOR UNIQUE] No unique variant for: " +
                    originalDef.defName);

                return null;
            }

            if (variantDef == null ||
                !variantDef.HasComp<CompArmorTraits>() ||
                !variantDef.HasComp<CompQuality>())
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] Invalid unique variant: " +
                    originalDef.defName);

                return null;
            }

            if (variantDef.MadeFromStuff)
            {
                if (stuffDef == null ||
                    !stuffDef.IsStuff ||
                    stuffDef.stuffProps == null ||
                    variantDef.stuffCategories == null ||
                    !variantDef.stuffCategories.Any(category =>
                        stuffDef.stuffProps.categories.Contains(category)))
                {
                    Log.Warning(
                        "[UET ARMOR UNIQUE] Invalid material: " +
                        (stuffDef != null ? stuffDef.defName : "NULL") +
                        " for " + variantDef.defName);

                    return null;
                }
            }
            else
            {
                if (stuffDef != null)
                {
                    Log.Warning(
                        "[UET ARMOR UNIQUE] Variant does not accept material: " +
                        variantDef.defName);
                }

                stuffDef = null;
            }

            ThingWithComps armor = null;

            try
            {
                UETDevArmorSpawnContext.Begin(
                    variantDef, trait, quality);

                armor = ThingMaker.MakeThing(
                    variantDef, stuffDef) as ThingWithComps;
            }
            catch (Exception exception)
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] Creation failed: " +
                    exception);
                return null;
            }
            finally
            {
                UETDevArmorSpawnContext.End();
            }

            if (armor == null)
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] Created apparel is null.");
                return null;
            }

            CompQuality qualityComp =
                armor.TryGetComp<CompQuality>();

            CompArmorTraits armorComp =
                armor.TryGetComp<CompArmorTraits>();

            if (qualityComp == null ||
                armorComp == null ||
                !armorComp.Traits.Contains(trait))
            {
                Log.Error(
                    "[UET ARMOR UNIQUE TEST] Initialization failed.");

                armor.Destroy();
                return null;
            }

            // Enforce the quality selected for developer testing
            qualityComp.SetQuality(
                quality,
                ArtGenerationContext.Outsider);

            // Assign a random author to dev-spawned unique armor
            UETArtAuthorUtility.EnsureRandomAuthor(armor);

            Log.Message(
                "[UET ARMOR UNIQUE TEST] Created: " +
                armor.def.defName +
                " | Quality: " + qualityComp.Quality +
                " | Trait: " + trait.defName);

            return armor;
        }
    }
}

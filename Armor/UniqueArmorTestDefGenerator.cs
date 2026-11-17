
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Generates and registers unique armor variants from eligible apparel definitions.
    /// Adds UET armor traits, Odyssey unique item support, and artistic naming rules.
    /// Other mods can call Generate() to register compatible armor variants.
    /// </summary>
    public static class UniqueArmorTestDefGenerator
    {
        private static readonly MethodInfo CloneMethod =
            typeof(object).GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        public static void Generate(bool hotReload = false)
        {
            RulePackDef armorDescription =
                DefDatabase<RulePackDef>.GetNamedSilentFail(
                    "UET_ArtDescription_Armor");

            if (armorDescription == null)
            {
                Log.Error(
                    "[UET ARMOR ART] Missing RulePackDef: " +
                    "UET_ArtDescription_Armor");

                return;
            }

            List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefs
                .Where(IsPreResolveCandidate)
                .OrderBy(def => def.defName)
                .ToList();

            foreach (ThingDef original in candidates)
            {
                string variantName =
                    UniqueVariantNameGenerator.GetVariantName(
                        original, "Armor");

                ThingDef existing =
                    DefDatabase<ThingDef>
                        .GetNamedSilentFail(variantName);

                if (existing != null)
                {
                    UniqueArmorVariantRegistry.Register(
                        original, existing);

                    continue;
                }

                ThingDef variant =
                    (ThingDef)CloneMethod.Invoke(
                        original, null);

                variant.defName = variantName;

                // Prevent UET variants from appearing in ordinary pawn equipment generation
                variant.generateCommonality = 0f;
                variant.generateAllowChance = 0f;

                variant.comps =
                    original.comps != null
                        ? new List<CompProperties>(
                            original.comps)
                        : new List<CompProperties>();

                if (!variant.comps.Any(comp =>
                    comp is CompProperties_ArmorTraits))
                {
                    variant.comps.Add(
                        new CompProperties_ArmorTraits());
                }

                if (!variant.comps.Any(comp =>
                    comp is CompProperties_UniqueWeapon))
                {
                    variant.comps.Add(
                        new CompProperties_UniqueWeapon
                        {
                            weaponCategories = new List<WeaponCategoryDef>(),
                            namerLabels = new List<string>
                            {
                                original.label ?? original.defName
                            }
                        });
                }

                CompProperties_Art existingArt =
                    variant.comps
                        .OfType<CompProperties_Art>()
                        .FirstOrDefault();

                if (existingArt != null)
                {
                    // Clone to avoid modifying the original CompArt
                    CompProperties_Art variantArt =
                        (CompProperties_Art)CloneMethod.Invoke(
                            existingArt, null);

                    variantArt.descriptionMaker = armorDescription;
                    variantArt.minQualityForArtistic =
                        QualityCategory.Masterwork;

                    int index = variant.comps.IndexOf(existingArt);
                    variant.comps[index] = variantArt;
                }
                else
                {
                    // Preserve the existing naming rules for now
                    CompProperties_Art sourceArt =
                        DefDatabase<ThingDef>.AllDefs
                            .Where(def => def.comps != null)
                            .SelectMany(def => def.comps)
                            .OfType<CompProperties_Art>()
                            .FirstOrDefault(comp =>
                                comp.nameMaker != null &&
                                comp.descriptionMaker != null &&
                                !comp.mustBeFullGrave);

                    if (sourceArt != null)
                    {
                        variant.comps.Add(
                            new CompProperties_Art
                            {
                                nameMaker = sourceArt.nameMaker,
                                descriptionMaker = armorDescription,
                                minQualityForArtistic =
                                    QualityCategory.Masterwork
                            });
                    }
                    else
                    {
                        Log.Warning(
                            "[UET ARMOR ART] No suitable name rules found: " +
                            variant.defName);
                    }
                }

                // Exclude unique from the Odyssey generator until handled by UET generation
                variant.thingSetMakerTags =
                    original.thingSetMakerTags != null
                        ? new List<string>(
                            original.thingSetMakerTags)
                        : new List<string>();

                variant.thingSetMakerTags.RemoveAll(
                    tag => tag == "UniqueWeapon");

                variant.modContentPack =
                    original.modContentPack;

                DefGenerator.AddImpliedDef(
                    variant, hotReload);

                UniqueArmorVariantRegistry.Register(
                    original, variant);
            }
        }

        private static bool IsPreResolveCandidate(
            ThingDef def)
        {
            return def != null &&
                   def.defName != null &&
                   !def.defName.StartsWith(
                       "UET_Unique_",
                       StringComparison.Ordinal) &&
                   def.apparel != null &&
                   def.thingClass != null &&
                   typeof(Apparel).IsAssignableFrom(
                       def.thingClass) &&
                   def.HasComp<CompQuality>() &&
                   !def.HasComp<CompUniqueWeapon>() &&
                   def.apparel.bodyPartGroups != null &&
                   def.apparel.bodyPartGroups.Count > 0;
        }
    }

    /// <summary>
    /// Finds apparel definitions eligible for UET unique armor generation.
    /// Excludes existing unique items and filters by armor trait eligibility.
    /// Other mods can use GetEligibleApparel() to discover compatible equipment.
    /// </summary>
    public static class UniqueArmorCandidateFinder
    {
        public static List<ThingDef> GetEligibleApparel()
        {
            return DefDatabase<ThingDef>.AllDefs
                .Where(def =>
                    def != null &&
                    def.defName != null &&
                    def.apparel != null &&
                    def.HasComp<CompQuality>() &&
                    !def.HasComp<CompUniqueWeapon>() &&
                    !def.defName.StartsWith(
                        "UET_Unique_",
                        StringComparison.Ordinal) &&
                    ArmorTraitEligibility
                        .CanReceiveArmorTrait(def))
                .OrderBy(def => def.label)
                .ToList();
        }
    }
}

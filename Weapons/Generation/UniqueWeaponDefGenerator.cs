
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Generates implied unique weapon variants for eligible melee weapons.
    /// Preserves the original weapon definitions while adding Odyssey
    /// unique weapon support and registering the generated variants.
    /// </summary>
    public static class UniqueWeaponDefGenerator
    {
        private static readonly MethodInfo CloneMethod =
            typeof(object).GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        public static void Generate(bool hotReload = false)
        {
            UniqueWeaponVariantRegistry.Clear();
            
            List<ThingDef> originals =
                DefDatabase<ThingDef>.AllDefs
                    .Where(IsEligible)
                    .ToList();

            foreach (ThingDef original in originals)
            {
                try
                {
                    string variantName = GetVariantName(original);

                    ThingDef variant =
                        DefDatabase<ThingDef>
                            .GetNamedSilentFail(variantName);

                    if (variant == null)
                    {
                        variant = CreateVariant(original);

                        if (variant == null)
                            continue;

                        DefGenerator.AddImpliedDef(
                            variant, hotReload);

                        variant =
                            DefDatabase<ThingDef>
                                .GetNamedSilentFail(variantName);
                    }

                    if (variant == null)
                        continue;

                    UniqueWeaponVariantRegistry.Register(
                        original, variant);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        "[UET] Failed to generate variant for " +
                        original.defName + ": " + exception);
                }
            }
        }

        private static bool IsEligible(ThingDef def)
        {
            return def != null &&
                   def.defName != null &&
                   !def.defName.StartsWith(
                       "UET_Unique_", StringComparison.Ordinal) &&
                   def.IsMeleeWeapon &&
                   def.HasComp<CompQuality>() &&
                   !def.HasComp<CompUniqueWeapon>();
        }

        private static ThingDef CreateVariant(ThingDef original)
        {
            CompProperties_UniqueWeapon uniqueProperties =
                UniqueWeaponCompFactory.Create(original);

            if (uniqueProperties == null)
                return null;

            ThingDef variant =
                (ThingDef)CloneMethod.Invoke(
                    original, null);

            variant.defName = GetVariantName(original);
            // Prevent unique variants from ordinary pawn equipment generation
            variant.generateCommonality = 0f;
            variant.generateAllowChance = 0f;

            // Copy the list without modifying the original
            variant.comps = original.comps != null
                ? new List<CompProperties>(original.comps)
                : new List<CompProperties>();

            bool hasUETComp = variant.comps.Any(
                comp => comp != null &&
                        comp.compClass == typeof(CompWeaponTrait));

            if (!hasUETComp)
            {
                variant.comps.Add(
                    new CompProperties
                    {
                        compClass = typeof(CompWeaponTrait)
                    });
            }

            variant.comps.Add(uniqueProperties);

            // Uniques sellable
            variant.tradeability = Tradeability.Sellable;

            // Mod ownership
            variant.modContentPack = original.modContentPack;

            // Copy the tag list without modifying the original
            variant.thingSetMakerTags =
                original.thingSetMakerTags != null
                    ? new List<string>(original.thingSetMakerTags)
                    : new List<string>();

            if (!variant.thingSetMakerTags.Contains("UniqueWeapon"))
            {
                variant.thingSetMakerTags.Add("UniqueWeapon");
            }

            return variant;
        }

        private static string GetVariantName(ThingDef original)
        {
            return UniqueVariantNameGenerator.GetVariantName(
                original, "Weapon");
        }
    }
}

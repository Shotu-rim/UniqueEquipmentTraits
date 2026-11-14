
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Core
{
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

            int generated = 0;
            int failed = 0;

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
                        {
                            failed++;
                            continue;
                        }

                        DefGenerator.AddImpliedDef(
                            variant, hotReload);

                        variant =
                            DefDatabase<ThingDef>
                                .GetNamedSilentFail(variantName);
                    }

                    if (variant == null)
                    {
                        failed++;
                        continue;
                    }

                    UniqueWeaponVariantRegistry.Register(
                        original, variant);

                    generated++;
                }
                catch (Exception exception)
                {
                    failed++;

                    Log.Error(
                        "[UET] Failed to generate variant for " +
                        original.defName + ": " + exception);
                }
            }

            Log.Message(
                "[UET] Registered unique variants: " +
                generated + ", failed: " + failed);
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

            // Component list. Existing CompProperties read-only.
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


            // Sellable Uniques
            variant.tradeability = Tradeability.Sellable;

            // Just in case
            variant.modContentPack = original.modContentPack;

            // Independent tag list — do not modify the original weapon.
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
            string packageId =
                original.modContentPack != null
                    ? original.modContentPack.PackageId
                    : "unknown";

            string identity =
                packageId + ":" + original.defName;

            byte[] bytes =
                Encoding.UTF8.GetBytes(identity);

            byte[] hash;

            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            StringBuilder suffix = new StringBuilder();

            for (int i = 0; i < 12; i++)
                suffix.Append(hash[i].ToString("x2"));

            return "UET_Unique_" + suffix + "_Weapon";
        }
    }
}

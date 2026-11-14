
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UniqueArmorTestDefGenerator
    {
        private static readonly MethodInfo CloneMethod =
            typeof(object).GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        public static void Generate(bool hotReload = false)
        {
            int registered = 0;
            int skipped = 0;

            List<ThingDef> candidates =
                DefDatabase<ThingDef>.AllDefs
                    .Where(IsPreResolveCandidate)
                    .OrderBy(def => def.defName)
                    .ToList();

            foreach (ThingDef original in candidates)
            {
                string variantName =
                    "UET_Unique_" +
                    original.defName +
                    "_Armor";

                ThingDef existing =
                    DefDatabase<ThingDef>
                        .GetNamedSilentFail(variantName);

                if (existing != null)
                {
                    UniqueArmorVariantRegistry.Register(
                        original, existing);

                    skipped++;
                    continue;
                }

                ThingDef variant =
                    (ThingDef)CloneMethod.Invoke(
                        original, null);

                variant.defName = variantName;

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

                // Komponent Odyssey dodamy dopiero
                // po zabezpieczeniu naturalnego generatora.
                // Na razie wariant nie może trafić
                // do puli losowania Odyssey.

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

                registered++;
            }

            Log.Message(
                "[UET ARMOR UNIQUE] Candidates: " +
                candidates.Count +
                " | Registered: " + registered +
                " | Existing: " + skipped);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LudeonTK;
using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>Read-only audit for UET generation, registry and dev-spawner filters.</summary>
    public static class UETDevWeaponDiagnostics
    {
        [DebugAction("Unique Equipment Traits", "Audit melee weapon definitions", actionType = DebugActionType.Action)]
        public static void Audit()
        {
            List<ThingDef> all = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d != null).ToList();
            List<ThingDef> originals = all.Where(d =>
                d.IsMeleeWeapon && d.defName != null &&
                !d.defName.StartsWith("UET_Unique_", StringComparison.Ordinal)).ToList();
            List<ThingDef> variants = all.Where(d => d.defName != null &&
                d.defName.StartsWith("UET_Unique_", StringComparison.Ordinal)).ToList();
            List<RimWorld.WeaponTraitDef> uetTraits = DefDatabase<RimWorld.WeaponTraitDef>.AllDefsListForReading
                .Where(t => t != null && t.GetModExtension<UETTraitExtension>() != null &&
                    t.canGenerateAlone && t.weaponCategory != null).ToList();

            int eligible = 0, excluded = 0, missing = 0, registryMissing = 0;
            int brokenComp = 0, noTraits = 0, menuMissing = 0, menuDuplicates = 0;
            int ownUnique = 0, noQuality = 0;
            var matchedVariants = new HashSet<ThingDef>();
            Log.Message("[UET Diagnostics] === UET weapon audit (read-only) ===");
            Log.Message("[UET Diagnostics] Loaded melee originals=" + originals.Count +
                "; UET-prefixed defs=" + variants.Count + "; compatible UET trait defs=" + uetTraits.Count);

            foreach (ThingDef original in originals.OrderBy(d => d.defName, StringComparer.Ordinal))
            {
                string mod = original.modContentPack != null ? original.modContentPack.Name : "Unknown";
                string info = original.defName + " | " + original.label + " | mod=" + mod;
                // Exact generator eligibility predicate (for a non-UET melee definition).
                if (!original.HasComp<CompQuality>())
                {
                    excluded++; noQuality++;
                    Log.Message("[UET Diagnostics] [EXCLUDED:No CompQuality] " + info);
                    continue;
                }
                if (original.HasComp<CompUniqueWeapon>())
                {
                    excluded++; ownUnique++;
                    Log.Message("[UET Diagnostics] [EXCLUDED:Already unique] " + info);
                    continue;
                }

                eligible++;
                string expectedName = ExpectedVariantName(original);
                ThingDef variant = DefDatabase<ThingDef>.GetNamedSilentFail(expectedName);
                if (variant == null)
                {
                    missing++;
                    Log.Warning("[UET Diagnostics] [MISSING VARIANT] " + info + " | expected=" + expectedName);
                    continue;
                }
                matchedVariants.Add(variant);

                ThingDef registered;
                bool registeredOk = UniqueWeaponVariantRegistry.TryGetVariant(original, out registered)
                    && ReferenceEquals(registered, variant);
                if (!registeredOk)
                {
                    registryMissing++;
                    Log.Warning("[UET Diagnostics] [REGISTRY MISMATCH] " + info +
                        " | expected=" + expectedName +
                        " | actual=" + (registered != null ? registered.defName : "null"));
                }

                CompProperties_UniqueWeapon props = variant.comps == null ? null :
                    variant.comps.OfType<CompProperties_UniqueWeapon>().FirstOrDefault();
                if (props == null || !variant.HasComp<CompUniqueWeapon>() ||
                    !variant.HasComp<CompQuality>())
                {
                    brokenComp++;
                    Log.Warning("[UET Diagnostics] [BROKEN COMPONENTS] " + info +
                        " | variant=" + expectedName);
                }

                bool compatible = props != null && props.weaponCategories != null &&
                    uetTraits.Any(t => props.weaponCategories.Contains(t.weaponCategory));
                if (!compatible)
                {
                    noTraits++;
                    Log.Warning("[UET Diagnostics] [NO COMPATIBLE UET TRAIT] " + info +
                        " | variant=" + expectedName);
                }

                // Mirrors UETDevWeaponSpawnerWindow constructor listing
                bool inMenu = original.IsWeapon && original.HasComp<CompQuality>() &&
                    (original.IsMeleeWeapon || original.IsRangedWeapon) &&
                    (original.HasComp<CompUniqueWeapon>() ||
                     UniqueWeaponVariantRegistry.TryGetVariant(original, out registered));
                if (!inMenu)
                {
                    menuMissing++;
                    Log.Warning("[UET Diagnostics] [NOT IN SPAWNER MENU] " + info);
                }
            }

            // Standalone rows
            foreach (ThingDef variant in variants)
            {
                ThingDef ignoredVariant;
                bool inMenu = variant.IsWeapon && variant.HasComp<CompQuality>() &&
                    (variant.IsMeleeWeapon || variant.IsRangedWeapon) &&
                    (variant.HasComp<CompUniqueWeapon>() ||
                     UniqueWeaponVariantRegistry.TryGetVariant(variant, out ignoredVariant));
                if (inMenu) menuDuplicates++;
                if (!matchedVariants.Contains(variant))
                    Log.Warning("[UET Diagnostics] [UNMATCHED UET VARIANT] " + variant.defName +
                        " | " + variant.label);
            }

            Log.Message("[UET Diagnostics] === SUMMARY ===");
            Log.Message("[UET Diagnostics] Eligible=" + eligible + "; excluded=" + excluded +
                " (no quality=" + noQuality + ", already unique=" + ownUnique + ")");
            Log.Message("[UET Diagnostics] Missing variants=" + missing +
                "; registry mismatches=" + registryMissing +
                "; broken components=" + brokenComp +
                "; no compatible UET trait=" + noTraits);
            Log.Message("[UET Diagnostics] Eligible originals absent from menu=" + menuMissing +
                "; UET variants ALSO listed as menu rows=" + menuDuplicates);
            Log.Message("[UET Diagnostics] Menu check models the window constructor, not live UI state. " +
                "Does not spawn weapons, mutate definitions or modify registry.");
        }

        // Mirrors UniqueWeaponDefGenerator.GetVariantName; in case
        private static string ExpectedVariantName(ThingDef original)
        {
            string packageId = original.modContentPack != null
                ? original.modContentPack.PackageId : "unknown";
            byte[] bytes = Encoding.UTF8.GetBytes(packageId + ":" + original.defName);
            byte[] hash;
            using (SHA256 sha = SHA256.Create()) hash = sha.ComputeHash(bytes);
            StringBuilder suffix = new StringBuilder();
            for (int i = 0; i < 12; i++) suffix.Append(hash[i].ToString("x2"));
            return "UET_Unique_" + suffix + "_Weapon";
        }
    }
}

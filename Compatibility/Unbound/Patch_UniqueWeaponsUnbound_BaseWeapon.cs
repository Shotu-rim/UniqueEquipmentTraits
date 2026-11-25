
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Compatibility.Unbound
{
    [HarmonyPatch]
    public static class Patch_UniqueWeaponsUnbound_BaseWeapon
    {
        private const string PackageId =
            "shunter.uniqueweaponsunbound";

        private static Dictionary<string, ThingDef> baseWeapons;

        public static bool Prepare()
        {
            return ModsConfig.IsActive(PackageId) &&
                   TargetMethod() != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                "UniqueWeaponsUnbound.WeaponRegistry:FindBaseWeapon",
                new Type[] { typeof(ThingDef) });
        }

        [HarmonyPostfix]
        public static void Postfix(
            ThingDef uniqueDef,
            ref ThingDef __result)
        {
            // UWU original result if successful
            if (__result != null || uniqueDef == null)
                return;

            string defName = uniqueDef.defName;

            // Only handle UET weapon variants
            if (string.IsNullOrEmpty(defName) ||
                !defName.StartsWith(
                    "UET_Unique_", StringComparison.Ordinal) ||
                !defName.EndsWith(
                    "_Weapon", StringComparison.Ordinal))
            {
                return;
            }

            if (baseWeapons == null)
                BuildLookup();

            ThingDef original;

            if (baseWeapons.TryGetValue(
                defName, out original))
            {
                __result = original;
            }
        }

        private static void BuildLookup()
        {
            Dictionary<string, ThingDef> lookup =
                new Dictionary<string, ThingDef>(
                    StringComparer.Ordinal);

            foreach (ThingDef original
                in DefDatabase<ThingDef>.AllDefs)
            {
                if (original == null ||
                    !original.IsMeleeWeapon ||
                    string.IsNullOrEmpty(original.defName))
                {
                    continue;
                }

                if (original.defName.StartsWith(
                    "UET_Unique_", StringComparison.Ordinal))
                {
                    continue;
                }

                string variantName =
                    UniqueVariantNameGenerator.GetVariantName(
                        original, "Weapon");

                if (!lookup.ContainsKey(variantName))
                    lookup.Add(variantName, original);
            }

            baseWeapons = lookup;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Compatibility.Unbound
{
    [HarmonyPatch]
    public static class Patch_PersonaWeaponsUnbound_BaseWeapon
    {
        private const string PackageId =
            "shunter.personaweaponsunbound";

        private static Dictionary<string, ThingDef> baseWeapons;

        public static bool Prepare()
        {
            return ModsConfig.IsActive(PackageId) &&
                   TargetMethod() != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                "PersonaWeaponsUnbound.WeaponRegistry:FindBaseWeapon",
                new Type[] { typeof(ThingDef) });
        }

        [HarmonyPostfix]
        public static void Postfix(
            ThingDef personaDef,
            ref ThingDef __result)
        {
            if (__result != null || personaDef == null)
                return;

            string defName = personaDef.defName;

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


using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.Unbound
{
    [HarmonyPatch]
    public static class Patch_UniqueWeaponsUnbound
    {
        private const string PackageId =
            "shunter.uniqueweaponsunbound";

        public static bool Prepare()
        {
            return ModsConfig.IsActive(PackageId) &&
                   TargetMethod() != null;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                "UniqueWeaponsUnbound.WeaponRegistry:" +
                "RegisterUniqueWeaponDef");
        }

        [HarmonyPrefix]
        public static bool Prefix(ThingDef def)
        {
            if (def == null ||
                string.IsNullOrEmpty(def.defName))
            {
                return true;
            }

            // UWU will not register UET armor
            if (def.defName.StartsWith(
                    "UET_Unique_",
                    StringComparison.Ordinal) &&
                def.defName.EndsWith(
                    "_Armor",
                    StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}

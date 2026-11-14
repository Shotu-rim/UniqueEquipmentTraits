
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(typeof(CompUniqueWeapon), "CanAddTrait")]
    public static class Patch_CompUniqueWeapon_CanAddTrait
    {
        [HarmonyPostfix]
        public static void Postfix(
            CompUniqueWeapon __instance,
            RimWorld.WeaponTraitDef trait,
            ref bool __result)
        {
            // Preserve Odyssey rejections
            if (!__result)
            {
                return;
            }

            if (__instance?.parent?.def == null)
            {
                return;
            }

            string defName = __instance.parent.def.defName;

            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            // Restrict UET generated weapons
            if (!defName.StartsWith(
                    "UET_Unique_",
                    System.StringComparison.Ordinal) ||
                !defName.EndsWith(
                    "_Weapon",
                    System.StringComparison.Ordinal))
            {
                return;
            }

            // UET weapons can receive only UET traits
            if (trait == null ||
                trait.GetModExtension<UETTraitExtension>() == null)
            {
                __result = false;
            }
        }
    }
}

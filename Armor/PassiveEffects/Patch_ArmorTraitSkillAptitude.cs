using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Adds equipped armor skill bonuses to the effective
    /// skill aptitude without modifying learned skill levels.
    /// </summary>
    [HarmonyPatch(typeof(SkillRecord), "Aptitude", MethodType.Getter)]
    public static class Patch_ArmorTraitSkillAptitude
    {
        public static void Postfix(
            SkillRecord __instance,
            ref int __result)
        {
            if (__instance == null)
                return;

            int bonus = ArmorTraitSkillUtility.GetSkillBonus(
                __instance.Pawn,
                __instance.def);

            if (bonus > 0)
                __result += bonus;
        }
    }
}
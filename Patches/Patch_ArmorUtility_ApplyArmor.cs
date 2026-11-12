
using HarmonyLib;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    internal static class Patch_ArmorUtility_ApplyArmor
    {
        [HarmonyPrefix]
        private static void Prefix(
            ref float armorRating,
            Pawn pawn)
        {
            var context =
                Patch_Verb_MeleeAttackDamage.CurrentContext;

            if (context == null ||
                !context.ArmorPiercingActivated ||
                context.Victim != pawn)
                return;

            armorRating *=
                1f - Mathf.Clamp01(
                    context.ArmorPiercingStrength);
        }
    }
}

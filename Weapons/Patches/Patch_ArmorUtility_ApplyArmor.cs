using HarmonyLib;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Applies the armor-piercing effect of UET melee weapon traits.
    /// Reduces the defender's effective armor rating during attacks
    /// with an active armor-piercing effect.
    /// Records whether armor reduction was actually applied.
    /// </summary>
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
                context.Victim != pawn ||
                armorRating <= 0f)
                return;

            float strength = Mathf.Clamp01(
                context.ArmorPiercingStrength);

            if (strength <= 0f)
                return;

            float originalArmorRating = armorRating;

            armorRating *= 1f - strength;

            if (armorRating < originalArmorRating)
            {
                Patch_Verb_MeleeAttackDamage
                    .MarkArmorPiercingApplied();
            }
        }
    }
}
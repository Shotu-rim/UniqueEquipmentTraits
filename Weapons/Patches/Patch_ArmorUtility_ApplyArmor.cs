
using HarmonyLib;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Applies the armor-piercing effect of UET melee weapon traits.
    /// Reduces the defender's effective armor rating during attacks
    /// with an active armor-piercing effect.
    /// Other mods can use this patch as a reference for integrating
    /// armor penetration effects with UET's melee combat context.
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
                context.Victim != pawn)
                return;

            armorRating *=
                1f - Mathf.Clamp01(
                    context.ArmorPiercingStrength);
        }
    }
}

using System;
using CombatExtended;
using HarmonyLib;
using UniqueEquipmentTraits.Patches;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    internal static class CESunderingScope
    {
        [ThreadStatic]
        internal static float Strength;
    }

    [HarmonyPatch(
        typeof(ArmorUtilityCE),
        nameof(ArmorUtilityCE.GetAfterArmorDamage))]
    internal static class Patch_CE_Sundering_Scope
    {
        [HarmonyPrefix]
        private static void Prefix(
            DamageInfo originalDinfo,
            Pawn pawn,
            out float __state)
        {
            __state = CESunderingScope.Strength;

            CESunderingScope.Strength =
                Patch_Verb_MeleeAttackDamage
                    .GetArmorPiercingStrength(
                        pawn,
                        originalDinfo.Instigator);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            float __state)
        {
            CESunderingScope.Strength = __state;

            return __exception;
        }
    }

    [HarmonyPatch(
        typeof(ArmorUtilityCE),
        "TryPenetrateArmor")]
    internal static class Patch_CE_Sundering_Penetration
    {
        [HarmonyPrefix]
        private static void Prefix(
            ref float armorAmount,
            Thing armor)
        {
            float strength = CESunderingScope.Strength;

            if (strength <= 0f || armorAmount <= 0f)
                return;

            float originalArmor = armorAmount;

            armorAmount *= 1f - strength;
            if (armorAmount < originalArmor)
            {
                Patch_Verb_MeleeAttackDamage
                    .MarkArmorPiercingApplied();
            }

        }
    }
}
using System;
using CombatExtended;
using HarmonyLib;
using UniqueEquipmentTraits.Patches;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    /// <summary>
    /// Connects Combat Extended melee attacks to the shared
    /// UET weapon trait attack lifecycle.
    /// </summary>
    [HarmonyPatch(
        typeof(Verb_MeleeAttackCE),
        nameof(Verb_MeleeAttackCE.ApplyMeleeDamageToTarget))]
    internal static class Patch_CE_WeaponAttackObserver
    {
        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackCE __instance,
            LocalTargetInfo target,
            out object __state)
        {
            Patch_Verb_MeleeAttackDamage.BeginAttack(
                __instance.CasterPawn,
                target.Thing as Pawn,
                __instance.EquipmentSource as ThingWithComps,
                out __state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            object __state)
        {
            return Patch_Verb_MeleeAttackDamage.EndAttack(
                __exception,
                __state);
        }
    }
}
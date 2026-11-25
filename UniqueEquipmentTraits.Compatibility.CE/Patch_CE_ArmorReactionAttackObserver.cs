using System;
using CombatExtended;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    [HarmonyPatch(
        typeof(Verb_MeleeAttackCE),
        nameof(Verb_MeleeAttackCE.ApplyMeleeDamageToTarget))]
    internal static class Patch_CE_ArmorReactionAttackObserver
    {
        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackCE __instance,
            LocalTargetInfo target,
            out object __state)
        {
            __state = ArmorReactionAttackObserver.BeginAttack(
                __instance.CasterPawn,
                target.Thing as Pawn);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            object __state)
        {
            ArmorReactionAttackObserver.EndAttack(__state);

            return __exception;
        }
    }
}
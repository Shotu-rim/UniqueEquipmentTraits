
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Manages the lifecycle of UET melee attack contexts.
    /// Creates and tracks an attack context, prepares pre-damage trait
    /// effects, and processes trait activations and combat log entries
    /// after successful execution of the melee damage routine.
    /// Supports nested attack contexts using a thread-local stack.
    /// </summary>
    [HarmonyPatch(
        typeof(Verb_MeleeAttackDamage),
        "ApplyMeleeDamageToTarget")]
    internal static class Patch_Verb_MeleeAttackDamage
    {
        [ThreadStatic]
        private static Stack<MeleeAttackContext> activeAttacks;

        internal static MeleeAttackContext CurrentContext
        {
            get
            {
                if (activeAttacks == null ||
                    activeAttacks.Count == 0)
                    return null;

                return activeAttacks.Peek();
            }
        }

        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackDamage __instance,
            LocalTargetInfo target,
            out MeleeAttackContext __state)
        {
            // State belongs to this specific Harmony
            __state = null;

            if (activeAttacks == null)
                activeAttacks = new Stack<MeleeAttackContext>();

            Pawn attacker = __instance.CasterPawn;
            Pawn victim = target.Thing as Pawn;

            ThingWithComps weapon =
                __instance.EquipmentSource as ThingWithComps;

            MeleeAttackContext context =
                new MeleeAttackContext(attacker, victim, weapon);

            // Register context before effects
            activeAttacks.Push(context);
            __state = context;

            // Select Prismatic's trait for attack
            PrismaticEffect.Prepare(context);

            // Prepare Sundering
            SunderingEffect.Prepare(context);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            MeleeAttackContext __state)
        {
            if (__state == null)
                return __exception;

            try
            {
                // Effect wont happen after failed attack
                if (__exception == null)
                {
                    WeaponTraitActivation.ProcessMeleeAttack(__state);
                    SunderingEffect.RecordCombatLog(__state);
                }
            }
            finally
            {
                // Remove context belonging to this call
                if (activeAttacks != null &&
                    activeAttacks.Count > 0 &&
                    ReferenceEquals(
                        activeAttacks.Peek(), __state))
                {
                    activeAttacks.Pop();
                }
            }

            return __exception;
        }
    }
}

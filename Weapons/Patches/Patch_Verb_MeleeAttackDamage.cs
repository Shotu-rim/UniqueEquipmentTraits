using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Manages the lifecycle of UET melee attack contexts.
    /// Supports vanilla and external combat systems through
    /// a shared attack lifecycle API.
    /// </summary>
    [HarmonyPatch(
        typeof(Verb_MeleeAttackDamage),
        "ApplyMeleeDamageToTarget")]
    public static class Patch_Verb_MeleeAttackDamage
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

        public static float GetArmorPiercingStrength(
            Pawn victim,
            Thing instigator)
        {
            MeleeAttackContext context = CurrentContext;

            if (context == null ||
                !context.ArmorPiercingActivated ||
                victim == null ||
                instigator == null ||
                context.Victim != victim ||
                context.Attacker != instigator)
                return 0f;

            return UnityEngine.Mathf.Clamp01(
                context.ArmorPiercingStrength);
        }

        public static void MarkArmorPiercingApplied()
        {
            MeleeAttackContext context = CurrentContext;

            if (context == null ||
                !context.ArmorPiercingActivated)
                return;

            context.ArmorPiercingApplied = true;
        }

        // Shared attack lifecycle API
        public static void BeginAttack(
            Pawn attacker,
            Pawn victim,
            ThingWithComps weapon,
            out object state)
        {
            state = null;

            if (activeAttacks == null)
                activeAttacks =
                    new Stack<MeleeAttackContext>();

            MeleeAttackContext context =
                new MeleeAttackContext(
                    attacker,
                    victim,
                    weapon);

            activeAttacks.Push(context);
            state = context;

            // Select Prismatic's trait for the attack.
            PrismaticEffect.Prepare(context);

            // Prepare armor penetration modifiers.
            SunderingEffect.Prepare(context);
        }

        public static Exception EndAttack(
            Exception exception,
            object state)
        {
            MeleeAttackContext context =
                state as MeleeAttackContext;

            if (context == null)
                return exception;

            try
            {
                if (exception == null)
                {
                    // Preserve original attack damage for Hawkeye.
                    context.CaptureOriginalDamage();

                    // Trait-generated damage must not modify
                    // the original melee contact information.
                    context.CloseContactObservation();

                    try
                    {
                        WeaponTraitActivation
                            .ProcessMeleeAttack(context);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            "[UET] Melee trait activation failed: "
                            + ex);
                    }

                    try
                    {
                        ArmorTraitCriticalEffect.Apply(context);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            "[UET Hawkeye] Critical hit processing failed: "
                            + ex);
                    }

                    try
                    {
                        SunderingEffect.RecordCombatLog(context);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            "[UET] Sundering combat log failed: "
                            + ex);
                    }
                }
            }
            finally
            {
                if (activeAttacks != null &&
                    activeAttacks.Count > 0 &&
                    ReferenceEquals(
                        activeAttacks.Peek(), context))
                {
                    activeAttacks.Pop();
                }
            }

            return exception;
        }

        // Vanilla adapter
        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackDamage __instance,
            LocalTargetInfo target,
            out object __state)
        {
            BeginAttack(
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
            return EndAttack(
                __exception,
                __state);
        }
    }
}

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Tracks melee attack contexts independently of weapon traits.
    /// Uses a thread-local stack to associate injuries with the
    /// correct attack and safely restore nested attack contexts.
    /// UET extensions can access Current to inspect the active attack.
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    internal static class ArmorReactionAttackObserver
    {
        [ThreadStatic]
        private static Stack<ArmorReactionContext> active;

        internal static ArmorReactionContext Current
        {
            get
            {
                return active != null && active.Count > 0
                    ? active.Peek()
                    : null;
            }
        }

        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackDamage __instance,
            LocalTargetInfo target,
            out ArmorReactionContext __state)
        {
            __state = new ArmorReactionContext(
                __instance.CasterPawn,
                target.Thing as Pawn);

            if (active == null)
                active = new Stack<ArmorReactionContext>();

            active.Push(__state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            ArmorReactionContext __state)
        {
            if (__state != null &&
                active != null &&
                active.Count > 0 &&
                ReferenceEquals(active.Peek(), __state))
            {
                active.Pop();
            }

            return __exception;
        }
    }

    /// <summary>
    /// Observes injuries produced during tracked melee attacks.
    /// Forwards matching injury events to the armor reaction system,
    /// allowing custom reactive armor traits to respond to hits.
    /// </summary>
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "FinalizeAndAddInjury",
        new Type[]
        {
            typeof(Pawn),
            typeof(Hediff_Injury),
            typeof(DamageInfo),
            typeof(DamageWorker.DamageResult)
        })]
    internal static class ArmorReactionInjuryObserver
    {
        [HarmonyPrefix]
        private static void Prefix(
            Pawn pawn,
            Hediff_Injury injury,
            DamageInfo dinfo)
        {
            ArmorReactionContext context =
                ArmorReactionAttackObserver.Current;

            if (context == null ||
                context.Defender == null ||
                pawn != context.Defender ||
                injury == null ||
                injury.Part == null ||
                context.Attacker == null ||
                dinfo.Instigator != context.Attacker)
            {
                return;
            }

            context.ObservePart(
                injury.Part,
                dinfo,
                injury.Severity);
        }
    }
}

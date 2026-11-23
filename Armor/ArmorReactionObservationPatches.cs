
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(
        typeof(Verb_MeleeAttackDamage),
        "ApplyMeleeDamageToTarget")]
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

        internal static void ObserveAbsorbedHit(
            Pawn pawn,
            DamageInfo dinfo)
        {
            if (UETThornsReflection.IsReflecting)
                return;

            ArmorReactionContext context = Current;

            if (context == null ||
                context.Defender == null ||
                context.Attacker == null ||
                pawn != context.Defender ||
                dinfo.HitPart == null ||
                dinfo.Instigator != context.Attacker)
            {
                return;
            }

            context.ObservePart(
                dinfo.HitPart,
                dinfo,
                0f);
        }
    }

    [HarmonyPatch(
        typeof(DamageWorker_AddInjury),
        "FinalizeAndAddInjury",
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
            DamageInfo dinfo,
            out UETThornsReflection.InjuryFrame __state)
        {
            __state = UETThornsReflection.BeginInjury();

            if (UETThornsReflection.IsReflecting)
                return;

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

        [HarmonyPostfix]
        private static void Postfix(
    Pawn pawn,
    Hediff_Injury injury,
    float __result,
    UETThornsReflection.InjuryFrame __state)
        {
            ArmorReactionDamageObserver.ObserveInjury(
                pawn,
                injury != null ? injury.Part : null,
                __result);

            UETThornsReflection.Resolve(
                __state,
                __result);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            UETThornsReflection.InjuryFrame __state)
        {
            UETThornsReflection.EndInjury(__state);
            return __exception;
        }
    }
}

using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    // Separate Harmony owner: does not touch Weapon Traits' context or activation.
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    internal static class ArmorReactionAttackObserver
    {
        [ThreadStatic] private static Stack<ArmorReactionContext> active;

        internal static ArmorReactionContext Current
        {
            get { return active != null && active.Count > 0 ? active.Peek() : null; }
        }

        [HarmonyPrefix]
        private static void Prefix(Verb_MeleeAttackDamage __instance,
            LocalTargetInfo target, out ArmorReactionContext __state)
        {
            __state = new ArmorReactionContext(__instance.CasterPawn, target.Thing as Pawn);
            if (active == null) active = new Stack<ArmorReactionContext>();
            active.Push(__state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, ArmorReactionContext __state)
        {
            if (__state != null && active != null && active.Count > 0 &&
                ReferenceEquals(active.Peek(), __state)) active.Pop();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(DamageWorker_AddInjury), "FinalizeAndAddInjury",
        new Type[] { typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo),
            typeof(DamageWorker.DamageResult) })]
    internal static class ArmorReactionInjuryObserver
    {
        [HarmonyPrefix]
        private static void Prefix(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo)
        {
            
            ArmorReactionContext context = ArmorReactionAttackObserver.Current;
            if (context == null || context.Defender == null || pawn != context.Defender ||
                injury == null || injury.Part == null || context.Attacker == null ||
                dinfo.Instigator != context.Attacker) return;
            if (ArmorReactionDiagnostics.Enabled)
            {
                Log.Message(
                    "[UET ARMOR DEBUG 4G1]" +
                    " injurySeverity=" + injury.Severity.ToString("F4") +
                    " damageInfoAmount=" + dinfo.Amount.ToString("F4") +
                    " injuryDef=" +
                    (injury.def != null ? injury.def.defName : "null") +
                    " injuryPart=" +
                    (injury.Part != null ? injury.Part.Label : "null"));
            }
            context.ObservePart(injury.Part, dinfo, injury.Severity);
        }
    }
}

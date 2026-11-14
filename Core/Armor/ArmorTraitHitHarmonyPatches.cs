using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch]
    public static class Patch_UET_MeleeAttackSequence
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget");
        }

        [HarmonyPrefix]
        public static void Prefix(LocalTargetInfo target)
        {
            ArmorTraitHitSequenceRecorder.Begin(target.Thing as Pawn);
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            ArmorTraitHitSequenceRecorder.End();
            return __exception;
        }
    }

    [HarmonyPatch]
    public static class Patch_UET_GetExactHitPart
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(DamageWorker_AddInjury), "GetExactPartFromDamageInfo");
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BodyPartRecord __result)
        {
            ArmorTraitHitSequenceRecorder.Record(pawn, __result);
        }
    }

    [HarmonyPatch]
    public static class Patch_UET_DamageInfoProcessed
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(DamageWorker_AddInjury), "ApplyToPawn");
        }

        [HarmonyPrefix]
        public static void Prefix(DamageInfo dinfo, Pawn pawn)
        {
            ArmorTraitHitSequenceRecorder.RecordDamageInfo(pawn, dinfo);
        }
    }

    [HarmonyPatch]
    public static class Patch_UET_ApplyDamageToPart
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(DamageWorker_AddInjury), "ApplyDamageToPart");
        }

        [HarmonyPrefix]
        public static void Prefix(Pawn pawn)
        {
            ArmorTraitHitSequenceRecorder.RecordPartCall(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_UET_ArmorCheck
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ArmorUtility), "GetPostArmorDamage");
        }

        [HarmonyPrefix]
        public static void Prefix(Pawn pawn)
        {
            ArmorTraitHitSequenceRecorder.RecordArmorCheck(pawn);
        }
    }

    [HarmonyPatch]
    public static class Patch_UET_FinalizeInjury
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(DamageWorker_AddInjury),
                "FinalizeAndAddInjury",
                new Type[] { typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo), typeof(DamageWorker.DamageResult) });
        }

        [HarmonyPrefix]
        public static void Prefix(Pawn pawn, Hediff_Injury injury)
        {
            ArmorTraitHitSequenceRecorder.RecordInjury(pawn, injury);
        }
    }
}


using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Patches
{
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
                if (activeAttacks == null || activeAttacks.Count == 0)
                    return null;

                return activeAttacks.Peek();
            }
        }

        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackDamage __instance,
            LocalTargetInfo target)
        {
            if (activeAttacks == null)
                activeAttacks = new Stack<MeleeAttackContext>();

            Pawn attacker = __instance.CasterPawn;
            Pawn victim = target.Thing as Pawn;
            ThingWithComps weapon =
                __instance.EquipmentSource as ThingWithComps;

            activeAttacks.Push(
                new MeleeAttackContext(attacker, victim, weapon));
        }


        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (activeAttacks == null || activeAttacks.Count == 0)
                return __exception;

            MeleeAttackContext context = activeAttacks.Pop();

            if (__exception == null)
                WeaponTraitActivation.ProcessMeleeAttack(context);

            return __exception;
        }

    }
}

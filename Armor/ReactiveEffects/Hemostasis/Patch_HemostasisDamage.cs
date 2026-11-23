
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class Patch_HemostasisDamage
    {

        [System.ThreadStatic]
        private static Dictionary<Pawn, int> damageDepth;

        private static int EnterDamage(Pawn pawn)
        {
            if (damageDepth == null)
                damageDepth = new Dictionary<Pawn, int>();

            int depth;
            damageDepth.TryGetValue(pawn, out depth);

            damageDepth[pawn] = depth + 1;

            return depth;
        }

        private static void ExitDamage(Pawn pawn)
        {
            if (damageDepth == null || pawn == null)
                return;

            int depth;

            if (!damageDepth.TryGetValue(pawn, out depth))
                return;

            if (depth <= 1)
                damageDepth.Remove(pawn);
            else
                damageDepth[pawn] = depth - 1;
        }


        [HarmonyPrefix]
        public static void Prefix(
            Thing __instance,
            out float __state)
        {
            __state = -1f;

            Pawn pawn = __instance as Pawn;

            if (pawn == null)
                return;

            int previousDepth = EnterDamage(pawn);

            // Nested damage calls are handled by the outermost call
            if (previousDepth > 0)
                return;

            if (pawn.health == null ||
                pawn.health.Dead ||
                pawn.health.hediffSet == null)
            {
                return;
            }

            __state = pawn.health.hediffSet.BleedRateTotal;
        }


        [HarmonyPostfix]
        public static void Postfix(
            Thing __instance,
            float __state)
        {
            if (__state < 0f)
                return;

            Pawn pawn = __instance as Pawn;

            if (pawn == null ||
                pawn.health == null ||
                pawn.health.Dead)
            {
                return;
            }

            float currentBleedRate =
                pawn.health.hediffSet.BleedRateTotal;

            if (!HemostasisUtility.CrossedThreshold(
                __state,
                currentBleedRate))
            {
                return;
            }

            Apparel apparel;
            CompArmorTraits comp;
            ArmorTraitDef trait;

            if (!HemostasisArmorUtility.TryGetHemostasis(
                pawn,
                out apparel,
                out comp,
                out trait))
            {
                return;
            }

            if (comp.State == null ||
                comp.State.MaxCharges <= 0 ||
                !comp.State.HasAvailableCharge)
            {
                return;
            }

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return;

            float activationChance =
                HemostasisUtility.GetActivationChance(
                    quality,
                    currentBleedRate);

            if (activationChance <= 0f)
                return;

            // Consume one charge per activation attempt.
            if (!comp.State.TryConsumeCharge())
                return;

            bool activated = Rand.Chance(activationChance);

            if (!activated)
            {
                UETCombatLog.Record(
                    trait,
                    pawn,
                    "Medical nanobots attempted to activate, but failed to seal the bleeding.");

                return;
            }

            string bodyPartName;

            bool bleedingStopped =
                HemostasisEffectUtility.TryStopRandomBleeding(
                    pawn,
                    out bodyPartName);

            if (bleedingStopped)
            {
                UETCombatLog.Record(
                    trait,
                    pawn,
                    "Medical nanobots successfully sealed bleeding in the " +
                    bodyPartName + ".");
            }
            else
            {
                UETCombatLog.Record(
                    trait,
                    pawn,
                    "Medical nanobots activated, but no eligible bleeding wound was found.");
            }
        }

        [HarmonyFinalizer]
        public static System.Exception Finalizer(
            Thing __instance,
            System.Exception __exception)
        {
            Pawn pawn = __instance as Pawn;

            if (pawn != null)
                ExitDamage(pawn);

            return __exception;
        }

    }
}

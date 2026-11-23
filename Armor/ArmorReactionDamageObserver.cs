
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    internal static class ArmorReactionDamageObserver
    {
        internal sealed class InjuryRecord
        {
            internal BodyPartRecord Part;
            internal float Damage;
        }

        internal sealed class DamageFrame
        {
            internal Pawn Defender;
            internal DamageInfo DamageInfo;

            internal readonly List<InjuryRecord> Injuries =
                new List<InjuryRecord>();
        }

        [ThreadStatic]
        private static Stack<DamageFrame> active;

        [HarmonyPrefix]
        private static void Prefix(
            Thing __instance,
            DamageInfo dinfo,
            out DamageFrame __state)
        {
            __state = null;

            Pawn pawn = __instance as Pawn;

            if (pawn == null)
                return;

            __state = new DamageFrame
            {
                Defender = pawn,
                DamageInfo = dinfo
            };

            if (active == null)
                active = new Stack<DamageFrame>();

            active.Push(__state);
        }

        internal static void ObserveInjury(
            Pawn pawn,
            BodyPartRecord part,
            float damage)
        {
            if (active == null ||
                active.Count == 0 ||
                pawn == null ||
                part == null ||
                damage <= 0f ||
                UETThornsReflection.IsReflecting)
            {
                return;
            }

            DamageFrame frame = active.Peek();

            if (frame.Defender != pawn)
                return;

            frame.Injuries.Add(new InjuryRecord
            {
                Part = part,
                Damage = damage
            });
        }


        [HarmonyPostfix]
        private static void Postfix(DamageFrame __state)
        {
            ArmorReactionDamageProcessor.Process(__state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            DamageFrame __state)
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
}

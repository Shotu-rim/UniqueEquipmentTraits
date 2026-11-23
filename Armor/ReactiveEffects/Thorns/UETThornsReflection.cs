
using RimWorld;
using System;
using System.Collections.Generic;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class UETThornsReflection
    {
        internal sealed class InjuryFrame
        {
            internal ArmorTraitActivationContext Pending;
        }

        [ThreadStatic]
        private static Stack<InjuryFrame> frames;

        [ThreadStatic]
        private static bool reflecting;

        internal static bool IsReflecting
        {
            get { return reflecting; }
        }

        internal static InjuryFrame BeginInjury()
        {
            if (frames == null)
                frames = new Stack<InjuryFrame>();

            InjuryFrame frame = new InjuryFrame();
            frames.Push(frame);
            return frame;
        }

        internal static void EndInjury(InjuryFrame frame)
        {
            if (frame == null || frames == null ||
                frames.Count == 0)
                return;

            if (ReferenceEquals(frames.Peek(), frame))
                frames.Pop();
        }

        internal static void Register(
            ArmorTraitActivationContext context)
        {
            if (reflecting || context == null ||
                frames == null || frames.Count == 0)
                return;

            InjuryFrame frame = frames.Peek();

            if (frame.Pending == null)
                frame.Pending = context;
        }

        internal static bool CanRegister
        {
            get
            {
                return !reflecting &&
                       frames != null &&
                       frames.Count > 0 &&
                       frames.Peek().Pending == null;
            }
        }

        internal static void Resolve(
            InjuryFrame frame,
            float actualDamage)
        {
            if (frame == null || frame.Pending == null)
                return;

            ArmorTraitActivationContext context = frame.Pending;
            frame.Pending = null;

            if (reflecting || actualDamage <= 0f)
                return;

            Pawn attacker = context.Attacker;
            Pawn defender = context.Defender;

            if (attacker == null || defender == null ||
                attacker.Dead || !attacker.Spawned)
                return;

            float reflectedDamage =
                UETThornsUtility.CalculateReflection(
                    context.Apparel,
                    actualDamage);

            if (reflectedDamage <= 0f)
                return;

            reflecting = true;

            try
            {
                DamageInfo reflection = new DamageInfo(
                    DamageDefOf.Cut,
                    reflectedDamage,
                    instigator: defender);

                DamageWorker.DamageResult result =
                    attacker.TakeDamage(reflection);

                float damageDealt =
                    result != null
                        ? result.totalDamageDealt
                        : 0f;

                if (damageDealt > 0f)
                {

                    string description =
                        defender.LabelShort + "'s " +
                        context.Apparel.LabelShort +
                        " unleashes a vicious counterstrike of razor-sharp microspines, " +
                        "inflicting " +
                        damageDealt.ToString("0.#") +
                        " damage on " +
                        attacker.LabelShort + ".";

                    UETCombatLog.Record(
                        context.Trait,
                        defender,
                        description,
                        attacker);

                }
            }
            finally
            {
                reflecting = false;
            }
        }
    }
}

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UETShockUtility
    {
        private static readonly MethodInfo CanBeStunnedMethod =
            AccessTools.Method(
                typeof(StunHandler),
                "CanBeStunnedByDamage");

        private static readonly MethodInfo CanAdaptMethod =
            AccessTools.Method(
                typeof(StunHandler),
                "CanAdaptToDamage");

        private static readonly FieldInfo AdaptationField =
            AccessTools.Field(
                typeof(StunHandler),
                "adaptationTicksLeft");

        public static bool CanShock(Pawn target)
        {
            if (target == null ||
                target.Destroyed ||
                !target.Spawned ||
                target.Dead ||
                target.Downed ||
                target.stances == null ||
                target.stances.stunner == null)
            {
                return false;
            }

            StunHandler stunner = target.stances.stunner;

            if (stunner.Stunned)
                return false;

            DamageDef def = DamageDefOf.NerveStun;

            if (CanBeStunnedMethod == null ||
                CanAdaptMethod == null ||
                AdaptationField == null)
            {
                return false;
            }

            bool canBeStunned = (bool)
                CanBeStunnedMethod.Invoke(
                    stunner,
                    new object[] { def });

            if (!canBeStunned)
                return false;

            bool canAdapt = (bool)
                CanAdaptMethod.Invoke(
                    stunner,
                    new object[] { def });

            if (canAdapt)
            {
                Dictionary<DamageDef, int> adaptation =
                    AdaptationField.GetValue(stunner)
                        as Dictionary<DamageDef, int>;

                int ticks;

                if (adaptation != null &&
                    adaptation.TryGetValue(def, out ticks) &&
                    ticks > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ApplyShock(
            Pawn attacker,
            Pawn defender,
            int stunTicks)
        {
            if (!CanShock(attacker) ||
                defender == null ||
                stunTicks <= 0)
            {
                return false;
            }

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.NerveStun,
                stunTicks / 30f,
                instigator: defender);

            StunHandler stunner = attacker.stances.stunner;

            stunner.Notify_DamageApplied(dinfo);

            return stunner.Stunned;
        }

    }
}
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorDamageInterceptionUtility
    {
        private static bool TryInterceptDamage(
            Pawn pawn,
            float damage)
        {
            if (pawn == null || damage <= 0f)
                return false;

            return ArmorPassiveDamageUtility
                .TryInterceptAndRecord(pawn, damage);
        }

        public static bool InterceptPartDamage(
            DamageInfo dinfo,
            Pawn pawn,
            DamageWorker.DamageResult result,
            float damage)
        {
            if (!TryInterceptDamage(pawn, damage))
                return false;

            result.AddPart(pawn, dinfo.HitPart);
            result.deflected = true;

            ObserveAbsorbedHit(pawn, dinfo);

            return true;
        }

        public static void ObserveAbsorbedHit(
            Pawn pawn,
            DamageInfo dinfo)
        {
            ArmorReactionAttackObserver
                .ObserveAbsorbedHit(pawn, dinfo);
        }
    }
}
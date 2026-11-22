
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSystemshockContext
    {
        // Temporary context used while creating an explosion.
        private static bool active;
        private static Pawn caster;
        private static QualityCategory quality;

        public static bool IsActive
        {
            get { return active; }
        }

        public static void Begin(
            Pawn source,
            QualityCategory sourceQuality)
        {
            active = true;
            caster = source;
            quality = sourceQuality;
        }

        public static void RegisterExplosion(
            Explosion explosion)
        {
            if (!active ||
                explosion == null ||
                explosion.damType != DamageDefOf.EMP ||
                explosion.instigator != caster ||
                explosion.Map == null)
            {
                return;
            }

            ArmorTraitSystemshockMapComponent component =
                explosion.Map.GetComponent<
                    ArmorTraitSystemshockMapComponent>();

            if (component == null)
                return;

            ArmorTraitSystemshockExplosionData existing;

            if (component.TryGetExplosionData(
                explosion, out existing))
            {
                return;
            }

            component.RegisterExplosion(
                explosion,
                new ArmorTraitSystemshockExplosionData(
                    caster,
                    quality));
        }

        public static bool TryGetExplosionData(
            Explosion explosion,
            out ArmorTraitSystemshockExplosionData data)
        {
            data = null;

            if (explosion == null || explosion.Map == null)
                return false;

            ArmorTraitSystemshockMapComponent component =
                explosion.Map.GetComponent<
                    ArmorTraitSystemshockMapComponent>();

            return component != null &&
                component.TryGetExplosionData(
                    explosion, out data);
        }

        public static void RegisterAffectedPawn(
            Explosion explosion,
            Pawn pawn)
        {
            ArmorTraitSystemshockExplosionData data;

            if (TryGetExplosionData(explosion, out data))
            {
                data.RegisterAffectedPawn(pawn);
            }
        }

        public static void FinishExplosion(
            Explosion explosion)
        {
            ArmorTraitSystemshockExplosionData data;

            if (!TryGetExplosionData(explosion, out data))
                return;

            ArmorTraitSystemshockMapComponent component =
                explosion.Map.GetComponent<
                    ArmorTraitSystemshockMapComponent>();

            // Remove first to prevent duplicate penalties.
            component.RemoveExplosion(explosion);

            data.ApplyPenalties();
        }

        public static void CancelExplosion(
            Explosion explosion)
        {
            if (explosion == null || explosion.Map == null)
                return;

            ArmorTraitSystemshockMapComponent component =
                explosion.Map.GetComponent<
                    ArmorTraitSystemshockMapComponent>();

            if (component != null)
            {
                component.RemoveExplosion(explosion);
            }
        }

        public static void End()
        {
            active = false;
            caster = null;
        }
    }
}

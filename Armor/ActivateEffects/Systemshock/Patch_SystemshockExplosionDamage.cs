
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch]
    public static class Patch_SystemshockExplosionDamage
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(DamageWorker),
                "ExplosionDamageThing");
        }

        [HarmonyPrefix]
        public static void Prefix(
            Explosion explosion,
            Thing t,
            List<Thing> damagedThings,
            List<Thing> ignoredThings)
        {
            Pawn pawn = t as Pawn;

            if (pawn == null)
                return;

            ArmorTraitSystemshockExplosionData data;

            if (!ArmorTraitSystemshockContext.TryGetExplosionData(
                explosion, out data))
            {
                return;
            }

            // Skip objects already processed by this explosion.
            if (damagedThings.Contains(t))
                return;

            // Skip objects explicitly ignored by the explosion.
            if (ignoredThings != null && ignoredThings.Contains(t))
                return;

            // Only mechanoids and pawns with qualifying implants.
            if (!ArmorTraitSystemshockTargetUtility.HasElectronics(pawn))
                return;

            ArmorTraitSystemshockContext.RegisterAffectedPawn(
                explosion,
                pawn);
        }
    }
}

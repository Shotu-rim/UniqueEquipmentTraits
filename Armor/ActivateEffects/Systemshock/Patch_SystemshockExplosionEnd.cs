
using System.Reflection;
using HarmonyLib;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch]
    public static class Patch_SystemshockExplosionEnd
    {
        [HarmonyPatch]
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Explosion),
                "ExplosionEnded");
        }

        [HarmonyPostfix]
        public static void Postfix(Explosion __instance)
        {
            ArmorTraitSystemshockContext.FinishExplosion(
                __instance);
        }
    }

    [HarmonyPatch(
        typeof(Explosion),
        nameof(Explosion.DeSpawn))]
    public static class Patch_SystemshockExplosionCleanup
    {
        [HarmonyPrefix]
        public static void Prefix(Explosion __instance)
        {
            ArmorTraitSystemshockContext.CancelExplosion(
                __instance);
        }
    }
}


using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(Explosion),
        nameof(Explosion.StartExplosion))]
    public static class Patch_SystemshockExplosionStart
    {
        [HarmonyPrefix]
        public static void Prefix(Explosion __instance)
        {
            ArmorTraitSystemshockContext.RegisterExplosion(
                __instance);
        }
    }
}

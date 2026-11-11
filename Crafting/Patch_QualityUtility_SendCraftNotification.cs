
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Crafting
{
    [HarmonyPatch(
        typeof(QualityUtility),
        nameof(QualityUtility.SendCraftNotification))]
    public static class Patch_QualityUtility_SendCraftNotification
    {
        [HarmonyPrefix]
        private static bool Prefix(Thing thing)
        {
            return !Patch_GenRecipe_PostProcessProduct
                .DeferNotification(thing);
        }
    }
}

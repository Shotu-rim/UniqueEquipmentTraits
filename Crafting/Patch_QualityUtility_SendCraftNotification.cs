
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Crafting
{
    /// <summary>
    /// Intercepts vanilla crafting quality notifications.
    /// Defers notifications while UET processes a crafted melee weapon,
    /// allowing the final item to be reported after unique conversion.
    /// Preserves normal notification behavior outside active UET crafting contexts.
    /// </summary>
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

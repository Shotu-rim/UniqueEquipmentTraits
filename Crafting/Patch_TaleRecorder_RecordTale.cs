
using HarmonyLib;
using RimWorld;

namespace UniqueEquipmentTraits.Crafting
{
    /// <summary>
    /// Intercepts RimWorld's tale recording for crafted artwork.
    /// Defers CraftedArt tales during UET melee weapon conversion
    /// so they can reference the final unique weapon instead.
    /// Preserves normal tale recording outside active UET crafting contexts.
    /// </summary>
    [HarmonyPatch(
        typeof(TaleRecorder),
        nameof(TaleRecorder.RecordTale),
        new System.Type[]
        {
            typeof(TaleDef),
            typeof(object[])
        })]
    public static class Patch_TaleRecorder_RecordTale
    {
        [HarmonyPrefix]
        private static bool Prefix(
            TaleDef def,
            object[] args)
        {
            return !Patch_GenRecipe_PostProcessProduct
                .DeferCraftedArt(def, args);
        }
    }
}

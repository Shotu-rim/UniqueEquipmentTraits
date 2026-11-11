
using HarmonyLib;
using RimWorld;

namespace UniqueEquipmentTraits.Crafting
{
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

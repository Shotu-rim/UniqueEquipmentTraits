
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(typeof(CompUniqueWeapon), "PostPostMake")]
    public static class Patch_CompUniqueWeapon_PostPostMake_WorldQuality
    {
        [HarmonyPostfix]
        public static void Postfix(CompUniqueWeapon __instance)
        {
            if (!UETWorldConversionContext.IsActive)
                return;

            ThingWithComps parent = __instance.parent;

            if (parent == null || parent.def == null)
                return;

            // Armor has its own initialization path.
            if (!parent.def.IsMeleeWeapon)
                return;

            if (!parent.def.defName.StartsWith("UET_Unique_") ||
                !parent.def.defName.EndsWith("_Weapon"))
                return;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return;

            // Restore the quality of the original item.
            quality.SetQuality(
                UETWorldConversionContext.SourceQuality,
                ArtGenerationContext.Outsider);
            // Assign author
            UETArtAuthorUtility.EnsureRandomAuthor(parent);
        }
    }
}


using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Restores the source item's quality during world conversion
    /// of UET unique melee weapons.
    /// Runs after Odyssey's CompUniqueWeapon initialization and
    /// ensures that generated weapons have an art author.
    /// Does not affect armor or normal weapon generation.
    /// </summary>
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

            // Armor has different init path
            if (!parent.def.IsMeleeWeapon)
                return;

            if (!parent.def.defName.StartsWith("UET_Unique_") ||
                !parent.def.defName.EndsWith("_Weapon"))
                return;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return;

            // Restore original quality
            quality.SetQuality(
                UETWorldConversionContext.SourceQuality,
                ArtGenerationContext.Outsider);

            // Assign author if none exists
            UETArtAuthorUtility.EnsureRandomAuthor(parent);
        }
    }
}


using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(typeof(CompUniqueWeapon), "PostPostMake")]
    public static class Patch_CompUniqueWeapon_PostPostMake_Armor
    {

        public static bool Prefix(CompUniqueWeapon __instance)
        {
            ThingWithComps parent = __instance.parent;

            if (parent == null || parent.def == null)
                return true;

            if (!UniqueArmorInitializer.IsUniqueArmor(parent.def))
                return true;

            CompQuality quality =
                parent.TryGetComp<CompQuality>();

            CompArmorTraits armorComp =
                parent.TryGetComp<CompArmorTraits>();

            if (armorComp == null || quality == null)
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] Missing required components: " +
                    parent.def.defName);

                return false;
            }

            // Odyssey expects this list to exist.
            if (__instance.TraitsListForReading == null)
            {
                AccessTools.Field(
                    typeof(CompUniqueWeapon),
                    "traits")
                    .SetValue(
                        __instance,
                        new List<WeaponTraitDef>());
            }

            bool devSpawn =
                UETDevArmorSpawnContext.Matches(parent);

            if (devSpawn)
            {
                // Preserve the user's selections in Dev Spawner.
                quality.SetQuality(
                    UETDevArmorSpawnContext.SelectedQuality,
                    ArtGenerationContext.Outsider);

                ArmorTraitDef selectedTrait =
                    UETDevArmorSpawnContext.SelectedTrait;

                bool added =
                    selectedTrait != null &&
                    armorComp.AddTrait(selectedTrait);

                Log.Message(
                    "[UET ARMOR UNIQUE] Dev spawn: " +
                    parent.def.defName +
                    " | Trait: " +
                    (selectedTrait != null
                        ? selectedTrait.defName
                        : "null") +
                    " | Added: " + added);

                return false;
            }

            // Natural generation through Odyssey.
            quality.SetQuality(
                QualityUtility.GenerateQuality(
                    QualityGenerator.Super),
                ArtGenerationContext.Outsider);

            bool initialized =
                UniqueArmorInitializer.InitializeTraits(parent);

            Log.Message(
                "[UET ARMOR UNIQUE] Natural spawn: " +
                parent.def.defName +
                " | Quality: " + quality.Quality +
                " | Initialized: " + initialized);

            return false;
        }

    }
}

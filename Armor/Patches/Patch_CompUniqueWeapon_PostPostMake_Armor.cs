
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Core.Armor.Naming;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Initializes UET unique armor when Odyssey creates an item.
    /// Assigns quality and armor traits, generates a unique name,
    /// and synchronizes the artwork title.
    /// Other mods can use this patch as a reference for integrating
    /// custom armor with Odyssey's unique item system.
    /// </summary>
    [HarmonyPatch(typeof(CompUniqueWeapon), "PostPostMake")]
    public static class Patch_CompUniqueWeapon_PostPostMake_Armor
    {
        private static readonly FieldInfo NameField =
            AccessTools.Field(
                typeof(CompUniqueWeapon), "name");

        private static readonly FieldInfo TraitsField =
            AccessTools.Field(
                typeof(CompUniqueWeapon), "traits");

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
                    "[UET ARMOR UNIQUE] Missing components: " +
                    parent.def.defName);

                return false;
            }

            if (__instance.TraitsListForReading == null &&
                TraitsField != null)
            {
                TraitsField.SetValue(
                    __instance,
                    new List<WeaponTraitDef>());
            }

            bool devSpawn =
                UETDevArmorSpawnContext.Matches(parent);

            if (devSpawn)
            {
                quality.SetQuality(
                    UETDevArmorSpawnContext.SelectedQuality,
                    ArtGenerationContext.Outsider);
            }
            else if (UETWorldConversionContext.IsActive)
            {
                quality.SetQuality(
                    UETWorldConversionContext.SourceQuality,
                    ArtGenerationContext.Outsider);
            }
            else
            {
                quality.SetQuality(
                    QualityUtility.GenerateQuality(
                        QualityGenerator.Super),
                    ArtGenerationContext.Outsider);
            }

            if (devSpawn)
            {
                armorComp.AddTrait(
                    UETDevArmorSpawnContext.SelectedTrait);
            }
            else
            {
                UniqueArmorInitializer.InitializeTraits(parent);
            }

            if (armorComp.Traits.Count > 0)
            {
                AssignName(
                    __instance,
                    parent,
                    armorComp.Traits[0]);
            }

            return false;
        }

        private static void AssignName(
            CompUniqueWeapon uniqueComp,
            ThingWithComps armor,
            ArmorTraitDef trait)
        {
            if (uniqueComp == null ||
                armor == null ||
                trait == null)
            {
                return;
            }

            if (NameField == null)
            {
                Log.Error(
                    "[UET ARMOR NAME] Odyssey name field not found.");
                return;
            }

            string existing =
                NameField.GetValue(uniqueComp) as string;

            if (!string.IsNullOrWhiteSpace(existing))
                return;

            string generated =
                ArmorNameGenerator.Generate(armor, trait);

            if (string.IsNullOrWhiteSpace(generated))
            {
                Log.Warning(
                    "[UET ARMOR NAME] Generation failed for " +
                    armor.def.defName +
                    " | Trait: " + trait.defName);
                return;
            }

            NameField.SetValue(uniqueComp, generated);

            CompArt art = armor.TryGetComp<CompArt>();

            if (art != null)
            {
                art.Title = generated;
            }
        }
    }
}

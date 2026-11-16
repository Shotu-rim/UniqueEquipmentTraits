
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Core.Armor.Naming;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
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

            bool initialized;

            if (devSpawn)
            {
                initialized = armorComp.AddTrait(
                    UETDevArmorSpawnContext.SelectedTrait);
            }
            else
            {
                initialized =
                    UniqueArmorInitializer.InitializeTraits(parent);
            }
            Log.Message(
    "[UET ART DEBUG] Prefix reached | " +
    "Item: " + parent.def.defName +
    " | Traits: " + armorComp.Traits.Count +
    " | Existing name: " +
    (NameField != null
        ? (NameField.GetValue(__instance) as string ?? "(null)")
        : "(field missing)"));
            if (armorComp.Traits.Count > 0)
            {
                AssignName(
                    __instance,
                    parent,
                    armorComp.Traits[0]);
            }

            Log.Message(
                "[UET ARMOR UNIQUE] Natural spawn: " +
                parent.def.defName +
                " | Quality: " + quality.Quality +
                " | Initialized: " + initialized);

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

            Log.Message(
                "[UET ARMOR NAME] Generated: " +
                generated +
                " | Trait: " + trait.defName);

            LogArmorArtDiagnostic(
                armor,
                trait,
                generated,
                art);
        }

        private static void LogArmorArtDiagnostic(
            ThingWithComps armor,
            ArmorTraitDef trait,
            string generatedName,
            CompArt art)
        {
            string pawnName = "Not identified";

            // Diagnostic only: recognize possessive names.
            int possessiveIndex =
                generatedName.IndexOf(
                    "'s ",
                    StringComparison.Ordinal);

            if (possessiveIndex > 0)
            {
                pawnName = generatedName.Substring(
                    0, possessiveIndex);
            }

            string artTitle = "No CompArt";
            string history = "No CompArt";
            string taleStatus = "No CompArt";

            if (art != null)
            {
                artTitle = art.Title;
                taleStatus =
                    art.TaleRef != null
                        ? "Present"
                        : "Missing";

                try
                {
                    history = art.GetDescriptionPart();

                    if (string.IsNullOrWhiteSpace(history))
                        history = "(Empty description)";
                }
                catch (Exception ex)
                {
                    history =
                        "Description generation failed: " +
                        ex;
                }
            }

            CompQuality quality =
                armor.TryGetComp<CompQuality>();

            string qualityText =
                quality != null
                    ? quality.Quality.ToString()
                    : "Unknown";

            Log.Message(
                "\n========== UET ARMOR ART DIAGNOSTIC ==========\n" +
                "Item: " + armor.def.label + "\n" +
                "DefName: " + armor.def.defName + "\n" +
                "Quality: " + qualityText + "\n" +
                "Trait: " + trait.defName + "\n" +
                "Generated name: " + generatedName + "\n" +
                "Pawn name (inferred): " + pawnName + "\n" +
                "Art title: " + artTitle + "\n" +
                "TaleReference: " + taleStatus + "\n" +
                "History and author (vanilla text):\n" +
                history + "\n" +
                "==============================================");
        }
    }
}

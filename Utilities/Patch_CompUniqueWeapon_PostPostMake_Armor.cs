
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
        // Prywatne pole Odyssey, które przechowuje
        // nazwę i zapisuje ją w PostExposeData().
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

            // Zwykłe bronie Odyssey pozostają bez zmian.
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

            // Odyssey oczekuje niepustej listy traitów broni.
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

                ArmorTraitDef selectedTrait =
                    UETDevArmorSpawnContext.SelectedTrait;

                bool added =
                    selectedTrait != null &&
                    armorComp.AddTrait(selectedTrait);

                if (added)
                {
                    AssignName(__instance, parent, selectedTrait);
                }

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

            // Naturalne generowanie przez Odyssey.
            quality.SetQuality(
                QualityUtility.GenerateQuality(
                    QualityGenerator.Super),
                ArtGenerationContext.Outsider);

            bool initialized =
                UniqueArmorInitializer.InitializeTraits(parent);

            if (initialized && armorComp.Traits.Count > 0)
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

            // Nie nadpisujemy istniejącej nazwy.
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

            // Zachowujemy zachowanie Odyssey dla CompArt.
            CompArt art = armor.TryGetComp<CompArt>();

            if (art != null)
                art.Title = generated;

            Log.Message(
                "[UET ARMOR NAME] Generated: " +
                generated +
                " | Trait: " + trait.defName);
        }
    }
}


using HarmonyLib;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

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

            // Zwykłe bronie Odyssey i UET pozostają nietknięte.
            if (parent.def.apparel == null ||
                parent.def.defName == null ||
                !parent.def.defName.StartsWith("UET_Unique_") ||
                !parent.def.defName.EndsWith("_Armor"))
            {
                return true;
            }

            // Od tego miejsca obsługujemy wyłącznie
            // specjalny wariant pancerza UET.


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

            bool devSpawn =
                UETDevArmorSpawnContext.Matches(parent);

            ArmorTraitDef selectedTrait;
            QualityCategory selectedQuality;

            if (devSpawn)
            {
                // Equipment Spawner:
                // używamy traitu i jakości wybranych przez użytkownika.
                selectedTrait =
                    UETDevArmorSpawnContext.SelectedTrait;

                selectedQuality =
                    UETDevArmorSpawnContext.SelectedQuality;
            }
            else
            {
                // Tymczasowa ścieżka naturalnego generowania.
                // Pełne losowanie traitów dodamy później.
                selectedTrait =
                    DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                        "UET_TestReactive");

                selectedQuality =
                    QualityUtility.GenerateQuality(
                        QualityGenerator.Super);
            }

            if (selectedTrait == null)
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] No trait available for " +
                    parent.def.defName);

                return false;
            }

            quality.SetQuality(
                selectedQuality,
                ArtGenerationContext.Outsider);

            bool added = armorComp.AddTrait(selectedTrait);

            Log.Message(
                "[UET ARMOR UNIQUE] Created: " +
                parent.def.defName +
                " | Quality: " + quality.Quality +
                " | Trait: " + selectedTrait.defName +
                " | Added: " + added);


            // Pomijamy oryginalną inicjalizację Odyssey
            // wyłącznie dla unikalnego pancerza UET.
            return false;
        }
    }
}

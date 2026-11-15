
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(
        typeof(CompUniqueWeapon),
        nameof(CompUniqueWeapon.PostPostMake))]
    internal static class Patch_CompUniqueWeapon_PostPostMake_Armor
    {
        [HarmonyPrefix]
        private static bool Prefix(CompUniqueWeapon __instance)
        {
            ThingWithComps armor =
                __instance.parent as ThingWithComps;

            if (armor == null ||
                !UniqueArmorInitializer.IsUniqueArmor(armor.def))
            {
                // Ordinary weapons use Odyssey's original code.
                return true;
            }



            // Initialize Odyssey's internal trait list.
            // Armor does not use WeaponTraitDef.
            List<WeaponTraitDef> weaponTraits =
                __instance.TraitsListForReading;

            if (weaponTraits == null)
            {
                AccessTools.Field(
                    typeof(CompUniqueWeapon),
                    "traits")
                    .SetValue(
                        __instance,
                        new List<WeaponTraitDef>());
            }

            // Armor uses UET traits instead of WeaponTraitDef.
            UniqueArmorInitializer.InitializeTraits(armor);


            // Preserve Odyssey's unique-item quality generation.
            CompQuality qualityComp = armor.TryGetComp<CompQuality>();

            if (qualityComp != null)
            {
                qualityComp.SetQuality(
                    QualityUtility.GenerateQuality(
                        QualityGenerator.Super),
                    ArtGenerationContext.Outsider);
            }

            // Do not run Odyssey's weapon naming logic.
            return false;

        }
    }
}

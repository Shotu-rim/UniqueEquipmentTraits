
using System;
using System.Linq;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UniqueArmorPoolFilter
    {
        public static bool CanGenerate(ThingDef def)
        {
            if (def == null || !def.HasComp<CompUniqueWeapon>())
                return false;

            bool isUETArmor =
                UniqueArmorInitializer.IsUniqueArmor(def);

            bool isUETMelee =
                def.defName != null &&
                def.defName.StartsWith(
                    "UET_Unique_",
                    StringComparison.Ordinal) &&
                def.IsMeleeWeapon;

            // Nie ingerujemy w oryginalne unikatowe bronie Odyssey.
            if (!isUETArmor && !isUETMelee)
                return true;

            // Wariant UET musi mieć dostępny materiał,
            // jeżeli wymaga go jego definicja.
            if (def.MadeFromStuff &&
                !GenStuff.AllowedStuffsFor(
                    def,
                    TechLevel.Undefined,
                    checkAllowedInStuffGeneration: true
                ).Any())
            {
                return false;
            }

            // Unikatowy pancerz musi mieć przynajmniej
            // jeden kompatybilny trait.
            if (isUETArmor)
            {
                return DefDatabase<ArmorTraitDef>.AllDefs
                    .Any(trait =>
                        trait != null &&
                        ArmorTraitCompatibility.CanApply(
                            def, trait));
            }

            // Walidację traitów broni białej
            // dodamy po sprawdzeniu jej inicjalizatora.
            return true;
        }
    }
}

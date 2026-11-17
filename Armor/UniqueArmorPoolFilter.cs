
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Validates equipment definitions for unique item generation pools.
    /// Checks material availability for UET variants and trait compatibility
    /// for unique armor while preserving original Odyssey unique weapons.
    /// Other mods can use CanGenerate() to apply the same generation rules.
    /// </summary>
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

            // Use original Odyssey unique
            if (!isUETArmor && !isUETMelee)
                return true;

            // Required materials must have at least one valid material available
            if (def.MadeFromStuff &&
                !GenStuff.AllowedStuffsFor(
                    def,
                    TechLevel.Undefined,
                    checkAllowedInStuffGeneration: true
                ).Any())
            {
                return false;
            }

            // Unique armor requires at least one compatible armor trait.
            if (isUETArmor)
            {
                return DefDatabase<ArmorTraitDef>.AllDefs
                    .Any(trait =>
                        trait != null &&
                        ArmorTraitCompatibility.CanApply(
                            def, trait));
            }

            // Melee weapon trait validation added after its initializer
            return true;
        }
    }
}


using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Identifies UET-generated unique armor and initializes its traits.
    /// Randomly assigns one compatible armor trait to items without existing traits.
    /// Other mods can use IsUniqueArmor() and InitializeTraits()
    /// when integrating with UET's unique armor generation system.
    /// </summary>
    public static class UniqueArmorInitializer
    {

        public static bool IsUniqueArmor(ThingDef def)
        {
            return def != null &&
                   def.defName != null &&
                   def.defName.StartsWith(
                       "UET_Unique_",
                       System.StringComparison.Ordinal) &&
                   def.defName.EndsWith(
                       "_Armor",
                       System.StringComparison.Ordinal) &&
                   def.apparel != null &&
                   def.HasComp<CompArmorTraits>();
        }

        public static bool InitializeTraits(
            ThingWithComps armor)
        {
            if (armor == null)
                return false;

            CompArmorTraits comp =
                armor.TryGetComp<CompArmorTraits>();

            if (comp == null)
                return false;

            // Do not assign another trait.
            if (comp.Traits.Count > 0)
                return false;

            List<ArmorTraitDef> candidates =
                DefDatabase<ArmorTraitDef>.AllDefs
                    .Where(trait =>
                        trait != null &&
                        ArmorTraitCompatibility.CanApply(
                            armor.def, trait))
                    .ToList();

            if (candidates.Count == 0)
                return false;

            ArmorTraitDef selected =
                candidates.RandomElement();

            return comp.AddTrait(selected);
        }
    }
}

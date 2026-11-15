
using System.Linq;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UniqueArmorPoolFilter
    {
        public static bool CanGenerate(ThingDef def)
        {
            if (def == null)
                return false;

            if (!def.HasComp<CompUniqueWeapon>())
                return false;

            // Leave ordinary Odyssey and UET weapons unchanged.
            if (!UniqueArmorInitializer.IsUniqueArmor(def))
                return true;

            // Unique armor must have at least one compatible trait.
            return DefDatabase<ArmorTraitDef>.AllDefs
                .Any(trait =>
                    trait != null &&
                    ArmorTraitCompatibility.CanApply(
                        def, trait));
        }
    }
}


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

            // Nie ingerujemy w zwykłe unikalne bronie Odyssey.
            if (!UniqueArmorInitializer.IsUniqueArmor(def))
                return true;

            bool compatible =
                DefDatabase<ArmorTraitDef>.AllDefs
                    .Any(trait =>
                        trait != null &&
                        ArmorTraitCompatibility.CanApply(def, trait));

            Log.Message(
                "[UET ARMOR POOL] " +
                def.defName +
                " | Compatible: " + compatible);

            return compatible;
        }

    }
}

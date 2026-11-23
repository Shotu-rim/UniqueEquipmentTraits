
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class HemostasisArmorUtility
    {
        public static bool TryGetHemostasis(
            Pawn pawn,
            out Apparel apparel,
            out CompArmorTraits comp,
            out ArmorTraitDef trait)
        {
            apparel = null;
            comp = null;
            trait = null;

            if (pawn == null ||
                pawn.apparel == null ||
                pawn.apparel.WornApparel == null)
            {
                return false;
            }

            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (worn == null)
                    continue;

                CompArmorTraits armorComp =
                    worn.TryGetComp<CompArmorTraits>();

                if (armorComp == null ||
                    armorComp.Traits == null)
                {
                    continue;
                }

                foreach (ArmorTraitDef candidate in armorComp.Traits)
                {
                    if (candidate == null ||
                        candidate.defName != "UET_Hemostasis")
                    {
                        continue;
                    }

                    apparel = worn;
                    comp = armorComp;
                    trait = candidate;

                    return true;
                }
            }

            return false;
        }
    }
}

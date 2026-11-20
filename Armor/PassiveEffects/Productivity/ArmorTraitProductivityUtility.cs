
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitProductivityUtility
    {
        public static float GetWorkSpeedBonus(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            float totalBonus = 0f;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null || comp.Traits == null)
                    continue;

                QualityCategory quality;

                if (!apparel.TryGetQuality(out quality))
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null)
                        continue;

                    if (quality == QualityCategory.Masterwork)
                    {
                        totalBonus +=
                            trait.masterworkWorkSpeedBonus;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        totalBonus +=
                            trait.legendaryWorkSpeedBonus;
                    }
                }
            }

            return totalBonus;
        }
    }
}

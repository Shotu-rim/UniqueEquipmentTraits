
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitCapacityUtility
    {
        public static float GetBestCapacityBonus(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            float bestBonus = 0f;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null)
                    continue;

                QualityCategory quality;

                if (!apparel.TryGetQuality(out quality))
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null)
                        continue;

                    float bonus = 0f;

                    if (quality == QualityCategory.Masterwork)
                    {
                        bonus = trait.masterworkCapacityBonus;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        bonus = trait.legendaryCapacityBonus;
                    }

                    if (bonus > bestBonus)
                        bestBonus = bonus;
                }
            }

            return bestBonus;
        }
    }
}

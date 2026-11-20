
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitCraftsmanshipUtility
    {
        public static float GetUpgradeChance(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            float highestChance = 0f;

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

                    float chance = 0f;

                    if (quality == QualityCategory.Masterwork)
                    {
                        chance =
                            trait.masterworkCraftsmanshipChance;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        chance =
                            trait.legendaryCraftsmanshipChance;
                    }

                    if (chance > highestChance)
                        highestChance = chance;
                }
            }

            return UnityEngine.Mathf.Clamp01(highestChance);
        }
    }
}

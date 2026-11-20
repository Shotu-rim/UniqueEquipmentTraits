
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitNeutralityUtility
    {
        public static float GetBestBlockChance(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            float bestChance = 0f;

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
                            trait.masterworkMentalBreakBlockChance;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        chance =
                            trait.legendaryMentalBreakBlockChance;
                    }

                    if (chance > bestChance)
                        bestChance = chance;
                }
            }

            return UnityEngine.Mathf.Clamp01(bestChance);
        }

        public static bool ShouldBlockMentalBreak(Pawn pawn)
        {
            float chance = GetBestBlockChance(pawn);

            if (chance <= 0f)
                return false;

            return Rand.Chance(chance);
        }
    }
}

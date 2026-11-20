
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{

    internal static class ArmorTraitRegenerationUtility
    {
        private const float TicksPerSecond = 60f;
        private const float DiminishingFactor = 0.5f;

        internal static float GetHealingPerTick(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            List<float> healingRates = new List<float>();

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

                    float interval = 0f;

                    if (quality == QualityCategory.Masterwork)
                    {
                        interval =
                            trait.masterworkRegenerationInterval;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        interval =
                            trait.legendaryRegenerationInterval;
                    }

                    if (interval <= 0f)
                        continue;

                    float healingPerTick =
                        1f / (interval * TicksPerSecond);

                    healingRates.Add(healingPerTick);
                }
            }

            if (healingRates.Count == 0)
                return 0f;

            // Strongest regeneration effects first
            healingRates.Sort();
            healingRates.Reverse();

            float totalRate = 0f;
            float multiplier = 1f;

            foreach (float rate in healingRates)
            {
                totalRate += rate * multiplier;
                multiplier *= DiminishingFactor;
            }

            return totalRate;
        }
    }
}

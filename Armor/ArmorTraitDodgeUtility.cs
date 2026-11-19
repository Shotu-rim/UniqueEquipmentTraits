
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitDodgeUtility
    {
        private const float DiminishingFactor = 0.25f;

        private struct DodgeBonus
        {
            public float Multiplier;
            public float Flat;
        }

        public static float ApplyDodgeBonus(
            Pawn pawn,
            float baseDodge)
        {
            if (pawn?.apparel == null)
                return baseDodge;

            List<DodgeBonus> bonuses =
                new List<DodgeBonus>();

            foreach (Apparel apparel in
                pawn.apparel.WornApparel)
            {
                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null)
                    continue;

                QualityCategory quality;

                if (!apparel.TryGetQuality(out quality))
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.activationType !=
                            ArmorTraitActivationType.Passive)
                    {
                        continue;
                    }

                    DodgeBonus bonus = new DodgeBonus();

                    if (quality == QualityCategory.Masterwork)
                    {
                        bonus.Multiplier =
                            trait.masterworkDodgeMultiplier;

                        bonus.Flat =
                            trait.masterworkDodgeFlatBonus;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        bonus.Multiplier =
                            trait.legendaryDodgeMultiplier;

                        bonus.Flat =
                            trait.legendaryDodgeFlatBonus;
                    }
                    else
                    {
                        continue;
                    }

                    if (bonus.Multiplier <= 0f &&
                        bonus.Flat <= 0f)
                    {
                        continue;
                    }

                    bonuses.Add(bonus);
                }
            }

            if (bonuses.Count == 0)
                return baseDodge;

            // Strongest bonuses first
            bonuses.Sort((a, b) =>
            {
                float strengthA =
                    a.Multiplier + a.Flat;

                float strengthB =
                    b.Multiplier + b.Flat;

                return strengthB.CompareTo(strengthA);
            });

            float totalMultiplier = 0f;
            float totalFlat = 0f;

            for (int i = 0; i < bonuses.Count; i++)
            {
                float weight =
                    1f / (1f + DiminishingFactor * i);

                totalMultiplier +=
                    bonuses[i].Multiplier * weight;

                totalFlat +=
                    bonuses[i].Flat * weight;
            }

            return baseDodge * (1f + totalMultiplier)
                + totalFlat;
        }
    }
}

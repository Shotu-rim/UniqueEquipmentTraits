
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitMoveSpeedUtility
    {
        private const float MinimumMass = 1f;
        private const float MaximumMass = 15f;

        private const float MinimumEffectiveness = 0.40f;
        private const float MaximumEffectiveness = 0.75f;

        private const float DiminishingFactor = 0.25f;

        public static float GetItemBonus(
            Apparel apparel,
            ArmorTraitDef trait)
        {
            if (apparel == null || trait == null)
                return 0f;

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return 0f;

            float nominalBonus = 0f;

            if (quality == QualityCategory.Masterwork)
            {
                nominalBonus =
                    trait.masterworkMoveSpeedBonus;
            }
            else if (quality == QualityCategory.Legendary)
            {
                nominalBonus =
                    trait.legendaryMoveSpeedBonus;
            }

            if (nominalBonus <= 0f)
                return 0f;

            float mass = apparel.GetStatValue(
                StatDefOf.Mass);

            float clampedMass = Mathf.Clamp(
                mass,
                MinimumMass,
                MaximumMass);

            float normalizedMass =
                (clampedMass - MinimumMass) /
                (MaximumMass - MinimumMass);

            float effectiveness =
                MinimumEffectiveness +
                (MaximumEffectiveness - MinimumEffectiveness) *
                Mathf.Sqrt(normalizedMass);

            return nominalBonus * effectiveness;
        }

        public static float GetMoveSpeedBonus(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return 0f;

            List<float> bonuses = new List<float>();

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null)
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.activationType !=
                        ArmorTraitActivationType.Passive)
                    {
                        continue;
                    }

                    float bonus = GetItemBonus(
                        apparel,
                        trait);

                    if (bonus > 0f)
                        bonuses.Add(bonus);
                }
            }

            if (bonuses.Count == 0)
                return 0f;

            // Strongest bonuses first
            bonuses.Sort((a, b) => b.CompareTo(a));

            float totalBonus = 0f;

            for (int i = 0; i < bonuses.Count; i++)
            {
                float weight =
                    1f /
                    (1f + DiminishingFactor * i);

                totalBonus += bonuses[i] * weight;
            }

            return totalBonus;
        }
    }
}

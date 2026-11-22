
using RimWorld;

namespace UniqueEquipmentTraits.Core.Armor
{
    public partial class CompArmorTraits
    {
        public override float GetStatFactor(StatDef stat)
        {
            float factor = base.GetStatFactor(stat);

            if (traits == null || traits.Count == 0)
                return factor;

            bool isArmor =
                stat == StatDefOf.ArmorRating_Sharp ||
                stat == StatDefOf.ArmorRating_Blunt ||
                stat == StatDefOf.ArmorRating_Heat;

            bool isMass = stat == StatDefOf.Mass;

            if (!isArmor && !isMass)
                return factor;

            QualityCategory quality;

            if (parent == null ||
                !parent.TryGetQuality(out quality))
                return factor;

            foreach (ArmorTraitDef trait in traits)
            {
                if (trait == null)
                    continue;

                float bonus = 0f;
                float reduction = 0f;

                if (quality == QualityCategory.Masterwork)
                {
                    bonus = isArmor
                        ? trait.masterworkArmorRatingBonus
                        : trait.masterworkMassIncrease;

                    reduction = isArmor
                        ? trait.masterworkLightweightArmorPenalty
                        : trait.masterworkLightweightMassReduction;
                }
                else if (quality == QualityCategory.Legendary)
                {
                    bonus = isArmor
                        ? trait.legendaryArmorRatingBonus
                        : trait.legendaryMassIncrease;

                    reduction = isArmor
                        ? trait.legendaryLightweightArmorPenalty
                        : trait.legendaryLightweightMassReduction;
                }

                // Reinforced: increases armor rating or mass
                if (bonus > 0f)
                    factor *= 1f + bonus;

                // Lightweight: decreases armor rating or mass
                if (reduction > 0f)
                    factor *= 1f - UnityEngine.Mathf.Clamp01(reduction);
            }

            return factor;
        }
    }
}

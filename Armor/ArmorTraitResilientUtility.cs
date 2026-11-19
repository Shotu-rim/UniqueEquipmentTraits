
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    //UET_Resilient
    internal static class ArmorTraitResilientUtility
    {
        internal static void GetBonuses(
            Pawn pawn,
            out float immunityGainBonus,
            out float diseaseRiskReduction)
        {
            immunityGainBonus = 0f;
            diseaseRiskReduction = 0f;

            if (pawn == null || pawn.apparel == null)
                return;

            float bestStrength = 0f;

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

                    float gainBonus = 0f;
                    float riskReduction = 0f;

                    if (quality == QualityCategory.Masterwork)
                    {
                        gainBonus = trait.masterworkImmunityGainBonus;
                        riskReduction = trait.masterworkDiseaseRiskReduction;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        gainBonus = trait.legendaryImmunityGainBonus;
                        riskReduction = trait.legendaryDiseaseRiskReduction;
                    }

                    float strength = gainBonus + riskReduction;

                    if (strength > bestStrength)
                    {
                        bestStrength = strength;
                        immunityGainBonus = gainBonus;
                        diseaseRiskReduction = riskReduction;
                    }
                }
            }
        }
    }
}


using RimWorld;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Provides shared charge capacity rules for armor traits.
    /// </summary>
    public static class ArmorTraitChargeUtility
    {
        private const int MasterworkCharges = 8;
        private const int LegendaryCharges = 10;

        private const int NeuralDeafnessLegendaryCharges = 2;
        private const int BerserkMasterworkCharges = 2;
        private const int BerserkLegendaryCharges = 3;
        private const int MedicMasterworkCharges = 1;
        private const int MedicLegendaryCharges = 2;

        public static int GetMaxCharges(
            ArmorTraitDef trait,
            QualityCategory quality)
        {
            if (trait == null || !trait.usesCharges)
                return 0;

            // Neural Deafness:
            // Passive on Masterwork.
            // Passive + Silence on Legendary.
            if (trait.defName == "UET_NeuralDeafness")
            {
                return quality == QualityCategory.Legendary
                    ? NeuralDeafnessLegendaryCharges
                    : 0;
            }
            // Berserk:
            // Active on Masterwork and Legendary.
            // Uses the existing armor trait charge system.

            if (trait.defName == "UET_Berserk")
            {
                switch (quality)
                {
                    case QualityCategory.Masterwork:
                        return 2;

                    case QualityCategory.Legendary:
                        return 3;

                    default:
                        return 0;
                }
            }
            // Shredding
            if (trait.defName == "UET_Shredding")
            {
                if (quality == QualityCategory.Masterwork)
                    return 1;

                if (quality == QualityCategory.Legendary)
                    return 2;

                return 0;
            }
            // Stunning Pulse
            if (trait.defName == "UET_StunningPulse")
            {
                switch (quality)
                {
                    case QualityCategory.Masterwork:
                        return 2;

                    case QualityCategory.Legendary:
                        return 3;

                    default:
                        return 0;
                }
            }

            // Medic:
            // Limited charges.
            // Uses the existing armor trait charge regeneration system.
            if (trait.defName == "UET_Medic")
            {
                switch (quality)
                {
                    case QualityCategory.Masterwork:
                        return MedicMasterworkCharges;

                    case QualityCategory.Legendary:
                        return MedicLegendaryCharges;

                    default:
                        return 0;
                }
            }
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return MasterworkCharges;

                case QualityCategory.Legendary:
                    return LegendaryCharges;

                default:
                    return 0;
            }
        }
    }
}

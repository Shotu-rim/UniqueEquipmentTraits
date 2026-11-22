
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
        private const int TechnomancerMasterworkCharges = 1;
        private const int TechnomancerLegendaryCharges = 2;

        public static int GetMaxCharges(
            ArmorTraitDef trait,
            QualityCategory quality)
        {
            if (trait == null || !trait.usesCharges)
                return 0;

            // Neural Deafness:
            // Passive on Masterwork. Passive + Silence on Legendary.
            if (trait.defName == "UET_NeuralDeafness")
            {
                return quality == QualityCategory.Legendary
                    ? NeuralDeafnessLegendaryCharges
                    : 0;
            }

            // Technomancer: 

            if (trait.defName == "UET_Technomancer")
            {
                switch (quality)
                {
                    case QualityCategory.Masterwork:
                        return TechnomancerMasterworkCharges;

                    case QualityCategory.Legendary:
                        return TechnomancerLegendaryCharges;

                    default:
                        return 0;
                }
            }

            // Berserk

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

            // Overdrive
            if (trait.defName == "UET_Overdrive")
            {
                switch (quality)
                {
                    case QualityCategory.Masterwork:
                        return 1;

                    case QualityCategory.Legendary:
                        return 2;

                    default:
                        return 0;
                }
            }

            // Systemshock
            if (trait.defName == "UET_Systemshock")
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

            // Oppression
            if (trait.defName == "UET_Oppression")
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

            // Medic
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

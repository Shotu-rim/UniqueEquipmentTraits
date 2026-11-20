
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

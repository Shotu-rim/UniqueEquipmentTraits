
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Provides shared immunity calculations for armor traits.
    /// Multiple worn items never stack their immunity values.
    /// </summary>
    public static class ArmorTraitImmunityUtility
    {
        /// <summary>
        /// Returns the strongest immunity provided by the
        /// pawn's currently worn apparel.
        /// </summary>
        public static float GetBestImmunity(
            Pawn pawn,
            string traitDefName)
        {
            if (pawn == null ||
                pawn.apparel == null ||
                string.IsNullOrEmpty(traitDefName))
            {
                return 0f;
            }

            float best = 0f;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                float reduction =
                    GetItemImmunity(apparel, traitDefName);

                if (reduction > best)
                    best = reduction;

                if (best >= 1f)
                    return 1f;
            }

            return best;
        }

        /// <summary>
        /// Returns immunity provided by this specific
        /// apparel item, independently of other worn items.
        /// </summary>
        public static float GetItemImmunity(
            Apparel apparel,
            string traitDefName)
        {
            if (apparel == null ||
                string.IsNullOrEmpty(traitDefName))
            {
                return 0f;
            }

            CompArmorTraits comp =
                apparel.TryGetComp<CompArmorTraits>();

            if (comp == null || comp.Traits == null)
                return 0f;

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return 0f;

            float best = 0f;

            foreach (ArmorTraitDef trait in comp.Traits)
            {
                if (trait == null ||
                    trait.defName != traitDefName ||
                    trait.activationType !=
                        ArmorTraitActivationType.Passive)
                {
                    continue;
                }

                float reduction = 0f;

                if (quality == QualityCategory.Masterwork)
                {
                    reduction =
                        trait.masterworkImmunityReduction;
                }
                else if (quality == QualityCategory.Legendary)
                {
                    reduction =
                        trait.legendaryImmunityReduction;
                }

                reduction = Mathf.Clamp01(reduction);

                if (reduction > best)
                    best = reduction;
            }

            return best;
        }
    }
}

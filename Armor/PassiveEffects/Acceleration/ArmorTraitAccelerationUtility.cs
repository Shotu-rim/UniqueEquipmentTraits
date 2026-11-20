
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitAccelerationUtility
    {
        public static float GetCooldownMultiplier(Pawn pawn)
        {
            if (pawn == null ||
                pawn.apparel == null ||
                pawn.apparel.WornApparel == null)
            {
                return 1f;
            }

            float strongestReduction = 0f;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    comp.Traits == null)
                {
                    continue;
                }

                QualityCategory quality;

                if (!apparel.TryGetQuality(out quality))
                    continue;

                float reduction = 0f;

                if (quality == QualityCategory.Masterwork)
                    reduction = 0.15f;
                else if (quality == QualityCategory.Legendary)
                    reduction = 0.25f;
                else
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.meleeCooldownReduction <= 0f)
                    {
                        continue;
                    }

                    if (reduction > strongestReduction)
                        strongestReduction = reduction;
                }
            }

            return 1f - strongestReduction;
        }
    }
}

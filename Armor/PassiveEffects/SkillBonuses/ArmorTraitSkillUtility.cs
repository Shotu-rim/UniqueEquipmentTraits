using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Calculates skill bonuses granted by equipped armor traits.
    /// Only the highest UET bonus for each skill is applied.
    /// </summary>
    public static class ArmorTraitSkillUtility
    {
        public static int GetSkillBonus(
            Pawn pawn,
            SkillDef skill)
        {
            if (pawn == null ||
                skill == null ||
                pawn.apparel == null)
            {
                return 0;
            }

            int highestBonus = 0;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
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
                        trait.boostedSkill != skill)
                    {
                        continue;
                    }

                    int bonus = 0;

                    if (quality == QualityCategory.Legendary)
                    {
                        bonus = trait.legendarySkillBonus;
                    }
                    else if (quality == QualityCategory.Masterwork &&
                             !trait.legendaryOnly)
                    {
                        bonus = trait.masterworkSkillBonus;
                    }

                    if (bonus > highestBonus)
                        highestBonus = bonus;
                }
            }

            return highestBonus;
        }
    }
}
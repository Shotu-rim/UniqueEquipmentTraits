
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Calculates the combined critical hit chance and
    /// weighted critical damage bonus from equipped armor traits.
    /// </summary>
    internal static class ArmorTraitCriticalUtility
    {
        internal static void GetCriticalStats(
            Pawn pawn,
            out float criticalChance,
            out float criticalDamageBonus)
        {
            criticalChance = 0f;
            criticalDamageBonus = 0f;

            if (pawn == null || pawn.apparel == null)
                return;

            float weightedDamageSum = 0f;

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

                    float chance = 0f;
                    float damageBonus = 0f;

                    if (quality == QualityCategory.Masterwork)
                    {
                        chance = trait.masterworkCriticalChance;
                        damageBonus =
                            trait.masterworkCriticalDamageBonus;
                    }
                    else if (quality == QualityCategory.Legendary)
                    {
                        chance = trait.legendaryCriticalChance;
                        damageBonus =
                            trait.legendaryCriticalDamageBonus;
                    }

                    if (chance <= 0f)
                        continue;

                    criticalChance += chance;
                    weightedDamageSum += chance * damageBonus;
                }
            }

            if (criticalChance > 0f)
            {
                criticalDamageBonus =
                    weightedDamageSum / criticalChance;
            }
        }
    }
}

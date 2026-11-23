
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UETThornsUtility
    {
        public static float CalculateReflection(
            Apparel apparel,
            float actualDamage)
        {
            if (apparel == null || actualDamage <= 0f)
                return 0f;

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return 0f;

            float baseReflection;
            float massMultiplier;
            float maxMassBonus;

            switch (quality)
            {
                case QualityCategory.Masterwork:
                    baseReflection = 0.25f;
                    massMultiplier = 0.03f;
                    maxMassBonus = 0.15f;
                    break;

                case QualityCategory.Legendary:
                    baseReflection = 0.35f;
                    massMultiplier = 0.04f;
                    maxMassBonus = 0.20f;
                    break;

                default:
                    return 0f;
            }

            float mass = Mathf.Max(
                0f,
                apparel.GetStatValue(StatDefOf.Mass));

            float massBonus = Mathf.Min(
                maxMassBonus,
                massMultiplier * Mathf.Sqrt(mass));

            float reflectionFactor =
                baseReflection + massBonus;

            return actualDamage * reflectionFactor;
        }
    }
}

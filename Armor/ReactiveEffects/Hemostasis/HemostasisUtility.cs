
using RimWorld;
using UnityEngine;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Calculates Hemostasis activation thresholds and chances.
    /// Bleed rates use RimWorld's fractional representation:
    /// 1.0 = 100%.
    /// </summary>
    internal static class HemostasisUtility
    {
        private const float ThresholdInterval = 0.5f;

        public static int GetThreshold(float bleedRate)
        {
            if (bleedRate <= 0f)
                return 0;

            return Mathf.FloorToInt(
                bleedRate / ThresholdInterval + 0.00001f);
        }

        public static bool CrossedThreshold(
            float previousBleedRate,
            float currentBleedRate)
        {
            return GetThreshold(currentBleedRate) >
                   GetThreshold(previousBleedRate);
        }

        public static float GetActivationChance(
            QualityCategory quality,
            float bleedRate)
        {
            int multiplier = GetThreshold(bleedRate);

            float chance;

            switch (quality)
            {
                case QualityCategory.Masterwork:
                    chance = 0.15f + 0.03f * multiplier;
                    break;

                case QualityCategory.Legendary:
                    chance = 0.20f + 0.05f * multiplier;
                    break;

                default:
                    return 0f;
            }

            return Mathf.Clamp01(chance);
        }
    }
}

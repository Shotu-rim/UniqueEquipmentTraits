
using RimWorld;
using UnityEngine;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Crafting
{
    public static class UniqueCraftingChance
    {
        // Returns probability 0-1
        public static float Calculate(
            int skillLevel,
            QualityCategory quality,
            bool inspired)
        {
            if (skillLevel <= 0)
                return 0f;

            float qualityFactor =
                WeaponTraitQuality.GetActivationChance(quality);

            float basePercent =
                (skillLevel / 2f) *
                (int)quality *
                qualityFactor;

            float finalPercent = inspired
                ? basePercent
                : basePercent / 3f;

            return Mathf.Clamp01(finalPercent / 100f);
        }
    }
}

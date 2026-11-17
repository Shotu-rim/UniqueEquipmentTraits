
using RimWorld;
using UniqueEquipmentTraits.Core;
using UnityEngine;

namespace UniqueEquipmentTraits.Crafting
{
    /// <summary>
    /// Calculates the probability of generating a unique crafted weapon.
    /// Uses crafting skill, item quality, and the quality-based activation factor.
    /// Inspired Creativity applies the full calculated chance,
    /// while ordinary crafting uses one third of that chance.
    /// Returns probabilities clamped to the range of 0 to 1.
    /// Other mods can use Calculate() to evaluate UET crafting chances.
    /// </summary>
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

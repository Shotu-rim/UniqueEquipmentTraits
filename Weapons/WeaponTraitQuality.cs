
using RimWorld;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Provides quality-based activation chances and effect strengths
    /// used by UET weapon trait and crafting systems.
    /// Supports all vanilla quality levels, including those used
    /// by the crafting generator, independently of the quality
    /// restrictions applied to unique equipment generation.
    /// </summary>
    public static class WeaponTraitQuality
    {
        public static float GetActivationChance(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Awful: return 0.00f;
                case QualityCategory.Poor: return 0.05f;
                case QualityCategory.Normal: return 0.10f;
                case QualityCategory.Good: return 0.15f;
                case QualityCategory.Excellent: return 0.20f;
                case QualityCategory.Masterwork: return 0.25f;
                case QualityCategory.Legendary: return 0.30f;
                default: return 0f;
            }
        }

        public static float GetEffectStrength(QualityCategory quality)
        {
            float chance = GetActivationChance(quality);

            return chance > 0f ? 0.05f + chance : 0f;
        }
    }
}

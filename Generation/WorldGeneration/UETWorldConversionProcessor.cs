
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Processes equipment collections for UET world conversion.
    /// Applies quality-based conversion chances to eligible melee
    /// weapons and apparel, replacing successful conversions in place.
    /// Prevents repeated conversion attempts for the same item
    /// within a collection and preserves shared item references.
    /// </summary>
    public static class UETWorldConversionProcessor
    {
        private const float MasterworkChance = 0.50f;
        private const float LegendaryChance = 0.40f;

        public static void Process(List<Thing> things)
        {
            if (things == null || things.Count == 0)
                return;

            // One attempt per Thing
            HashSet<Thing> processed = new HashSet<Thing>();

            // Reuse the same replacement 
            Dictionary<Thing, Thing> replacements =
                new Dictionary<Thing, Thing>();

            for (int i = 0; i < things.Count; i++)
            {
                Thing original = things[i];

                if (original == null)
                    continue;

                if (replacements.TryGetValue(
                    original, out Thing existing))
                {
                    things[i] = existing;
                    continue;
                }

                // Minified items separate handling. Thing unchanged
                if (original is MinifiedThing)
                    continue;

                if (!processed.Add(original))
                    continue;

                if (!UETWorldConversionEligibility.IsEligible(original))
                    continue;

                CompQuality quality =
                    original.TryGetComp<CompQuality>();

                if (quality == null)
                    continue;

                float chance = GetChance(quality.Quality);

                // Only Masterwork and Legendary convert
                if (chance <= 0f)
                    continue;

                // Preserve the original random roll
                bool rollPassed = Rand.Chance(chance);

                if (!rollPassed)
                    continue;

                if (!UETWorldConversionThingFactory.TryCreate(
                    original, out Thing converted))
                {
                    continue;
                }

                replacements.Add(original, converted);
                things[i] = converted;
            }
        }

        private static float GetChance(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return MasterworkChance;

                case QualityCategory.Legendary:
                    return LegendaryChance;

                default:
                    return 0f;
            }
        }
    }
}

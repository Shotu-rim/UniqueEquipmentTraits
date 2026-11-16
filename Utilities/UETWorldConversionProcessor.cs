
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionProcessor
    {
        private const float MasterworkChance = 0.50f;
        private const float LegendaryChance = 0.40f;

        public static void Process(List<Thing> things)
        {
            if (things == null || things.Count == 0)
                return;

            // One attempt per Thing instance within this list.
            HashSet<Thing> processed = new HashSet<Thing>();

            // Reuse the same replacement if the same source
            // reference occurs multiple times.
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

                // Minified items need separate handling.
                // Do not replace their inner Thing here.
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


                if (Prefs.DevMode && original.def.IsMeleeWeapon)
                {
                    Log.Message(
                        "[UET][WORLD][WEAPON ROLL] " +
                        original.def.defName +
                        " | Quality: " + quality.Quality +
                        " | Chance: " + (chance * 100f) + "%");
                }

                if (chance <= 0f || !Rand.Chance(chance))
                    continue;


                if (!UETWorldConversionThingFactory.TryCreate(
                    original, out Thing converted))
                {
                    continue;
                }

                replacements.Add(original, converted);
                things[i] = converted;

                if (Prefs.DevMode)
                {
                    Log.Message(
                        "[UET][WORLD][CONVERTED] " +
                        original.def.defName +
                        " -> " + converted.def.defName +
                        " | Quality: " + quality.Quality);
                }
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

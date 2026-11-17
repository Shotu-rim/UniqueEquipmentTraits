
using System;
using System.Collections.Generic;

namespace UniqueEquipmentTraits.Core.Armor.Naming
{
    /// <summary>
    /// Provides vocabulary used to generate names for unique armor.
    /// Exposes shared neutral nouns and trait-specific adjectives and concepts.
    /// Other mods can supply custom naming vocabulary through ArmorTraitDef.
    /// </summary>
    public static class ArmorNameVocabulary
    {
        // Shared vocabulary of neutral proper names
        private static readonly List<string> NeutralNouns =
            new List<string>
            {
                "Aegis",
                "Beacon",
                "Crown",
                "Dawn",
                "Eclipse",
                "Ember",
                "Horizon",
                "Legacy",
                "Omen",
                "Pinnacle",
                "Requiem",
                "Sentinel",
                "Solstice",
                "Tempest",
                "Vanguard",
                "Whisper"
            };

        public static IReadOnlyList<string> GetNeutralNouns()
        {
            return NeutralNouns;
        }

        public static IReadOnlyList<string> GetAdjectives(
            ArmorTraitDef trait)
        {
            if (trait == null || trait.nameAdjectives == null)
                return Array.Empty<string>();

            return trait.nameAdjectives;
        }

        public static IReadOnlyList<string> GetConcepts(
            ArmorTraitDef trait)
        {
            if (trait == null || trait.nameConcepts == null)
                return Array.Empty<string>();

            return trait.nameConcepts;
        }
    }
}

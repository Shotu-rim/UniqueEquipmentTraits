
using System;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Core.Armor.Naming
{
    public static class ArmorNameVocabulary
    {
        // Wspólny słownik neutralnych nazw własnych.
        // Lista robocza, do późniejszego audytu.
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

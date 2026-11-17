
using RimWorld;
using System;
using System.Text.RegularExpressions;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor.Naming
{
    /// <summary>
    /// Provides naming utilities for unique armor.
    /// Formats names, retrieves item and material labels,
    /// generates fictional owners, and handles possessive forms.
    /// Other mods can reuse these methods for custom equipment naming.
    /// </summary>
    public static class ArmorNameUtility
    {
        public static string FormatUniqueName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Remove RimWorld formatting tags
            name = name.StripTags().Trim();

            // Normalize whitespace
            name = Regex.Replace(name, @"\s+", " ");

            // Apply English title capitalization
            string[] words = name.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (string.IsNullOrEmpty(word))
                    continue;

                // Keep short function words
                string lower = word.ToLowerInvariant();

                if (i > 0 &&
                    (lower == "of" ||
                     lower == "the" ||
                     lower == "and" ||
                     lower == "in" ||
                     lower == "on" ||
                     lower == "for"))
                {
                    words[i] = lower;
                    continue;
                }

                // Preserve internal capitalization
                words[i] =
                    char.ToUpperInvariant(word[0]) +
                    word.Substring(1);
            }

            return string.Join(" ", words);
        }

        // Return the base ThingDef label without Odyssey's unique name
        public static string GetThingLabel(Thing armor)
        {
            if (armor == null || armor.def == null)
                return null;

            return armor.def.label;
        }

        // Use the item's actual material, never a random one
        public static string GetMaterialLabel(Thing armor)
        {
            if (armor == null || armor.Stuff == null)
                return null;

            return armor.Stuff.label;
        }

        // Check whether the item label already starts
        public static bool HasMaterialPrefix(
            string thingLabel,
            string materialLabel)
        {
            if (string.IsNullOrWhiteSpace(thingLabel) ||
                string.IsNullOrWhiteSpace(materialLabel))
            {
                return false;
            }

            return thingLabel.StartsWith(
                materialLabel + " ",
                StringComparison.OrdinalIgnoreCase);
        }

        // Generate a fictional human name for unique armor
        public static string GetRandomPawnName()
        {
            Gender gender = Rand.Bool
                ? Gender.Male
                : Gender.Female;

            Name name = PawnBioAndNameGenerator.GenerateFullPawnName(
                ThingDefOf.Human,
                gender: gender,
                nameCategory: PawnNameCategory.HumanStandard,
                forceNoNick: true);

            if (name == null)
                return null;

            NameTriple triple = name as NameTriple;

            if (triple != null)
                return triple.First;

            return name.ToStringShort;
        }

        // Build the English possessive form without
        // duplicating an existing possessive suffix
        public static string MakePossessive(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            name = name.Trim();

            if (name.EndsWith("'s",
                StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("'"))
            {
                return name;
            }

            return name + "'s";
        }
    }
}

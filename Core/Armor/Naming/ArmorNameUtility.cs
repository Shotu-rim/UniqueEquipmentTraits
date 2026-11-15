
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Grammar;
using System.Text.RegularExpressions;

namespace UniqueEquipmentTraits.Core.Armor.Naming
{
    public static class ArmorNameUtility
    {

        public static string FormatUniqueName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Usuwamy znaczniki formatowania RimWorld.
            name = name.StripTags().Trim();

            // Normalizujemy odstępy.
            name = Regex.Replace(name, @"\s+", " ");

            // Angielski Title Case z małymi przyimkami.
            string[] words = name.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (string.IsNullOrEmpty(word))
                    continue;

                // Krótkie wyrazy funkcyjne pozostają małe,
                // chyba że rozpoczynają nazwę.
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

                // Nie zmieniamy reszty liter, aby nie uszkodzić
                // nazw własnych zawierających wewnętrzne wielkie litery.
                words[i] =
                    char.ToUpperInvariant(word[0]) +
                    word.Substring(1);
            }

            return string.Join(" ", words);
        }
        // Zwraca podstawową nazwę przedmiotu
        // z ThingDef, bez unikalnej nazwy Odyssey.
        public static string GetThingLabel(Thing armor)
        {
            if (armor == null || armor.def == null)
                return null;

            return armor.def.label;
        }

        // Materiał pobieramy wyłącznie z rzeczywistego
        // Thing.Stuff, nigdy z losowego słownika.
        public static string GetMaterialLabel(Thing armor)
        {
            if (armor == null || armor.Stuff == null)
                return null;

            return armor.Stuff.label;
        }

        // Sprawdza, czy nazwa przedmiotu już zawiera
        // nazwę materiału na początku.
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

        // Generuje fikcyjną postać analogicznie
        // do mechanizmu używanego przez Odyssey.

        public static string GetRandomPawnName()
        {
            return null;
        }


        // Buduje angielski dopełniacz imienia.
        // Nie dodaje drugiego apostrofu,
        // jeżeli nazwa już jest dzierżawcza.
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

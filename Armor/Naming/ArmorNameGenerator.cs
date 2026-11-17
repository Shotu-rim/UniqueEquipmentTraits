
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor.Naming
{
    /// <summary>
    /// Generates randomized names for unique armor using weighted patterns.
    /// Combines armor labels, materials, trait vocabulary, and fictional owners.
    /// Other mods can call Generate() to create names for compatible UET armor.
    /// </summary>
    public static class ArmorNameGenerator
    {
        private const float TraitShare = 0.85f;

        private sealed class NamePattern
        {
            public float Weight;
            public string Result;

            public NamePattern(float weight, string result)
            {
                Weight = weight;
                Result = result;
            }
        }

        public static string Generate(
            ThingWithComps armor,
            ArmorTraitDef trait)
        {
            if (armor == null || trait == null)
                return null;

            string item = ArmorNameUtility.GetThingLabel(armor);

            if (string.IsNullOrWhiteSpace(item))
                return null;

            item = item.Trim();

            string material =
                ArmorNameUtility.GetMaterialLabel(armor);

            bool hasMaterial =
                !string.IsNullOrWhiteSpace(material) &&
                !ArmorNameUtility.HasMaterialPrefix(
                    item, material);

            if (hasMaterial)
                material = material.Trim();

            IReadOnlyList<string> adjectives =
                ArmorNameVocabulary.GetAdjectives(trait);

            IReadOnlyList<string> concepts =
                ArmorNameVocabulary.GetConcepts(trait);

            IReadOnlyList<string> neutralNouns =
                ArmorNameVocabulary.GetNeutralNouns();

            string adjective = PickWord(adjectives);
            string concept = PickWord(concepts);
            string neutral = PickWord(neutralNouns);

            // Generate fictional owner per name
            string pawnName =
                ArmorNameUtility.GetRandomPawnName();

            string owner =
                ArmorNameUtility.MakePossessive(pawnName);

            List<NamePattern> traitPatterns =
                new List<NamePattern>();

            List<NamePattern> neutralPatterns =
                new List<NamePattern>();

            // Trait patterns: 85% of selection pool
            // Two basic patterns have weight reduced by 20%

            // 1. [traitAdj] [thingLabel]
            if (!string.IsNullOrWhiteSpace(adjective))
            {
                traitPatterns.Add(new NamePattern(
                    24f, adjective + " " + item));

                // 7. The [traitAdj] [thingLabel]
                traitPatterns.Add(new NamePattern(
                    8f, "The " + adjective + " " + item));

                // 5. [pawn]'s [traitAdj] [thingLabel]
                if (!string.IsNullOrWhiteSpace(owner))
                {
                    traitPatterns.Add(new NamePattern(
                        12f,
                        owner + " " + adjective + " " + item));
                }
            }

            // 2. [thingLabel] of [traitConcept]
            if (!string.IsNullOrWhiteSpace(concept))
            {
                traitPatterns.Add(new NamePattern(
                    20.8f, item + " of " + concept));

                // 3. [material] [thingLabel]
                //    of [traitConcept]
                if (hasMaterial)
                {
                    traitPatterns.Add(new NamePattern(
                        6f,
                        material + " " + item +
                        " of " + concept));
                }

                // 6. [pawn]'s [material] [thingLabel]
                //    of [traitConcept]
                if (hasMaterial &&
                    !string.IsNullOrWhiteSpace(owner))
                {
                    traitPatterns.Add(new NamePattern(
                        10f,
                        owner + " " + material + " " +
                        item + " of " + concept));
                }
            }

            // Neutral patterns: 15% of selection pool

            // 4. [pawn]'s [thingLabel]
            if (!string.IsNullOrWhiteSpace(owner))
            {
                neutralPatterns.Add(new NamePattern(
                    45f, owner + " " + item));
            }

            if (!string.IsNullOrWhiteSpace(neutral))
            {
                // 8. The [neutralNoun]
                neutralPatterns.Add(new NamePattern(
                    32f, "The " + neutral));

                // 9. [pawn]'s [neutralNoun]
                if (!string.IsNullOrWhiteSpace(owner))
                {
                    neutralPatterns.Add(new NamePattern(
                        23f, owner + " " + neutral));
                }
            }

            // Fall back to the other group if no valid patterns exist
            if (traitPatterns.Count == 0)
                return ArmorNameUtility.FormatUniqueName(
                    Choose(neutralPatterns));

            if (neutralPatterns.Count == 0)
                return ArmorNameUtility.FormatUniqueName(
                    Choose(traitPatterns));

            string result = Rand.Value < TraitShare
                ? Choose(traitPatterns)
                : Choose(neutralPatterns);

            return ArmorNameUtility.FormatUniqueName(result);
        }

        private static string PickWord(
            IReadOnlyList<string> words)
        {
            if (words == null || words.Count == 0)
                return null;

            List<string> valid = new List<string>();

            for (int i = 0; i < words.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(words[i]))
                    valid.Add(words[i].Trim());
            }

            if (valid.Count == 0)
                return null;

            return valid.RandomElement();
        }

        private static string Choose(
            List<NamePattern> patterns)
        {
            if (patterns == null || patterns.Count == 0)
                return null;

            float total = 0f;

            foreach (NamePattern pattern in patterns)
                total += pattern.Weight;

            if (total <= 0f)
                return null;

            float roll = Rand.Range(0f, total);

            foreach (NamePattern pattern in patterns)
            {
                roll -= pattern.Weight;

                if (roll <= 0f)
                    return pattern.Result;
            }

            return patterns[patterns.Count - 1].Result;
        }
    }
}

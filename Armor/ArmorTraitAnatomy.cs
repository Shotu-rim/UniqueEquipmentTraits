
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Classifies body parts into armor trait categories using anatomical groups,
    /// body part tags, and ancestor fallbacks for non-standard anatomies.
    /// Other mods can use GetCategory() to determine the appropriate armor
    /// category for custom body parts and armor trait compatibility.
    /// </summary>
    public static class ArmorTraitAnatomy
    {
        public static ArmorTraitCategory GetCategory(
            BodyPartRecord part)
        {
            if (part?.def == null)
                return ArmorTraitCategory.None;

            // Anatomical group has priority
            ArmorTraitCategory category =
                GetCategoryFromGroups(part);

            if (category != ArmorTraitCategory.None)
                return category;

            // Use tags of actual body part
            category = GetCategoryFromTags(part);

            if (category != ArmorTraitCategory.None)
                return category;

            // Fallback for non-standard body. Prefer ancestor groups over ancestor tags
            for (BodyPartRecord current = part.parent;
                 current != null;
                 current = current.parent)
            {
                category = GetCategoryFromGroups(current);

                if (category != ArmorTraitCategory.None)
                    return category;
            }

            for (BodyPartRecord current = part.parent;
                 current != null;
                 current = current.parent)
            {
                category = GetCategoryFromTags(current);

                if (category != ArmorTraitCategory.None)
                    return category;
            }

            return ArmorTraitCategory.None;
        }

        private static ArmorTraitCategory GetCategoryFromGroups(
            BodyPartRecord part)
        {
            if (part?.groups == null)
                return ArmorTraitCategory.None;

            foreach (BodyPartGroupDef group in part.groups)
            {
                if (group == null)
                    continue;

                switch (group.defName)
                {
                    case "UpperHead":
                    case "FullHead":
                        return ArmorTraitCategory.Helmet;

                    case "Neck":
                        return ArmorTraitCategory.Neck;

                    case "Shoulders":
                        return ArmorTraitCategory.Shoulders;

                    case "Torso":
                        return ArmorTraitCategory.Torso;

                    case "Arms":
                        return ArmorTraitCategory.Arms;

                    case "Hands":
                    case "LeftHand":
                    case "RightHand":
                        return ArmorTraitCategory.Hands;

                    case "Waist":
                        return ArmorTraitCategory.Waist;

                    case "Legs":
                        return ArmorTraitCategory.Legs;

                    case "Feet":
                        return ArmorTraitCategory.Feet;
                }
            }

            return ArmorTraitCategory.None;
        }

        private static ArmorTraitCategory GetCategoryFromTags(
            BodyPartRecord part)
        {
            if (part?.def == null)
                return ArmorTraitCategory.None;

            if (HasTag(part,
                BodyPartTagDefOf.ManipulationLimbDigit))
                return ArmorTraitCategory.Hands;

            if (HasTag(part,
                BodyPartTagDefOf.MovingLimbDigit))
                return ArmorTraitCategory.Feet;

            if (HasTag(part,
                    BodyPartTagDefOf.ManipulationLimbCore) ||
                HasTag(part,
                    BodyPartTagDefOf.ManipulationLimbSegment))
                return ArmorTraitCategory.Arms;

            if (HasTag(part,
                    BodyPartTagDefOf.MovingLimbCore) ||
                HasTag(part,
                    BodyPartTagDefOf.MovingLimbSegment))
                return ArmorTraitCategory.Legs;

            return ArmorTraitCategory.None;
        }

        private static bool HasTag(
            BodyPartRecord part,
            BodyPartTagDef tag)
        {
            return part?.def?.tags != null &&
                   tag != null &&
                   part.def.tags.Contains(tag);
        }
    }
}

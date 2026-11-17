
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Determines which body parts and anatomical categories are covered by armor.
    /// Excludes belts and shields, including supported modded shield types.
    /// Other mods can use CoversBodyPart() and GetCoveredCategories()
    /// to check armor coverage and trait compatibility.
    /// </summary>
    public static class ArmorTraitCoverage
    {
        public static bool CoversBodyPart(
            ThingDef apparelDef,
            BodyPartRecord bodyPart)
        {
            if (apparelDef?.apparel == null ||
                bodyPart == null)
                return false;

            if (apparelDef.thingClass == null ||
                !typeof(Apparel).IsAssignableFrom(
                    apparelDef.thingClass))
                return false;


            if (apparelDef.apparel.layers != null)
            {
                foreach (ApparelLayerDef layer
                    in apparelDef.apparel.layers)
                {
                    if (layer?.defName == "Belt")
                        return false;
                }
            }

            if (apparelDef.comps != null)
            {
                foreach (CompProperties comp in apparelDef.comps)
                {
                    string compName = comp?.compClass?.FullName;

                    if (compName == "RimWorld.CompShield" ||
                        compName == "VEF.Apparels.CompShield" ||
                        compName == "CompShieldBlock")
                    {
                        return false;
                    }
                }
            }

            if (apparelDef.thingClass.FullName ==
                "VEF.Apparels.Apparel_Shield")
            {
                return false;
            }


            // Hard shield-layer exclusion
            if (apparelDef.apparel.layers != null)
            {
                foreach (ApparelLayerDef layer
                    in apparelDef.apparel.layers)
                {
                    if (layer?.defName == "Shield_layer")
                        return false;
                }
            }

            // Coverage check
            return apparelDef.apparel.CoversBodyPart(bodyPart);
        }

        public static List<ArmorTraitCategory> GetCoveredCategories(
            ThingDef apparelDef,
            BodyDef body)
        {
            List<ArmorTraitCategory> result =
                new List<ArmorTraitCategory>();

            if (apparelDef == null || body?.AllParts == null)
                return result;

            foreach (BodyPartRecord part in body.AllParts)
            {
                if (!CoversBodyPart(apparelDef, part))
                    continue;

                ArmorTraitCategory category =
                    ArmorTraitAnatomy.GetCategory(part);

                if (category == ArmorTraitCategory.None)
                    continue;

                if (!result.Contains(category))
                    result.Add(category);
            }

            return result;
        }

    }
}

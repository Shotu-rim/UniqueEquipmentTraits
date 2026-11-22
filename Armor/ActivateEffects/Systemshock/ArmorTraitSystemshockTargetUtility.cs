
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSystemshockTargetUtility
    {
        public static bool HasElectronics(Pawn pawn)
        {
            if (pawn == null)
                return false;

            // Mechanoids always qualify.
            if (pawn.RaceProps != null && pawn.RaceProps.IsMechanoid)
                return true;

            if (pawn.health == null || pawn.health.hediffSet == null)
                return false;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_Implant))
                    continue;

                ThingDef implantThing = hediff.def.spawnThingOnRemoved;

                if (implantThing == null || implantThing.thingCategories == null)
                    continue;

                foreach (ThingCategoryDef category in implantThing.thingCategories)
                {
                    if (category.defName == "BodyPartsBionic" ||
                        category.defName == "BodyPartsProsthetic")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

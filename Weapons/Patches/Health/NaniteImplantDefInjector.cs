
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UniqueEquipmentTraits.Implants;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Injects nanite damage components into eligible implant definitions,
    /// including implants added by other mods.
    /// Targets non-organic added body parts with removable items
    /// of Industrial tech level or higher, while preventing
    /// duplicate component injection.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class NaniteImplantDefInjector
    {
        static NaniteImplantDefInjector()
        {
            Inject();
        }

        public static void Inject()
        {
            foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs.ToList())
            {
                if (def.hediffClass == null ||
                    !typeof(Hediff_AddedPart).IsAssignableFrom(def.hediffClass) ||
                    def.addedPartProps == null ||
                    def.organicAddedBodypart ||
                    def.spawnThingOnRemoved == null ||
                    def.spawnThingOnRemoved.techLevel < TechLevel.Industrial)
                {
                    continue;
                }

                if (def.comps == null)
                {
                    def.comps = new List<HediffCompProperties>();
                }

                if (def.comps.Any(comp =>
                    comp != null &&
                    comp.compClass == typeof(HediffComp_NaniteDamage)))
                {
                    continue;
                }

                def.comps.Add(new HediffCompProperties_NaniteDamage());
            }
        }
    }
}

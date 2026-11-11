
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Implants;

namespace UniqueEquipmentTraits.Core
{
    [StaticConstructorOnStartup]
    public static class NaniteImplantDefInjector
    {
        static NaniteImplantDefInjector()
        {
            Inject();
        }

        public static void Inject()
        {
            int injectedCount = 0;

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
                injectedCount++;

            }

            Log.Message(
                $"[UET] Nanite implant components injected: {injectedCount}"
            );
        }
    }
}

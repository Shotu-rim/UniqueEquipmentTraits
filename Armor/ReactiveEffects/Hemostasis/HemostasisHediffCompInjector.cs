
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class HemostasisHediffCompInjector
    {
        public static void Inject()
        {
            foreach (HediffDef def in
                DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (def.hediffClass == null)
                    continue;

                if (!typeof(Hediff_Injury).IsAssignableFrom(
                        def.hediffClass) &&
                    !typeof(Hediff_MissingPart).IsAssignableFrom(
                        def.hediffClass))
                {
                    continue;
                }

                if (def.comps == null)
                    def.comps = new List<HediffCompProperties>();

                bool hasComp = false;

                foreach (HediffCompProperties comp in def.comps)
                {
                    if (comp is HediffCompProperties_Hemostasis)
                    {
                        hasComp = true;
                        break;
                    }
                }

                if (hasComp)
                    continue;

                def.comps.Add(
                    new HediffCompProperties_Hemostasis());
            }
        }
    }
}

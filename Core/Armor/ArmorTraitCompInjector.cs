
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitCompInjector
    {
        public static void Inject()
        {
            int injected = 0;
            int existing = 0;

            foreach (ThingDef def in
                DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.apparel == null)
                    continue;

                // Korzystamy z istniejącego systemu
                // kwalifikacji wyposażenia.
                if (!ArmorTraitEligibility.CanReceiveArmorTrait(def))
                    continue;

                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                bool hasComp = false;

                foreach (CompProperties comp in def.comps)
                {
                    if (comp is CompProperties_ArmorTraits)
                    {
                        hasComp = true;
                        break;
                    }
                }

                if (hasComp)
                {
                    existing++;
                    continue;
                }

                def.comps.Add(new CompProperties_ArmorTraits());
                injected++;
            }

            Log.Message(
                "[UET ARMOR] CompArmorTraits injected: " +
                injected + ", already present: " + existing);
        }
    }
}

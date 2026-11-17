
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Injects the armor trait component into all eligible apparel definitions.
    /// Prevents duplicate components and preserves existing configurations.
    /// Other mods can call Inject() to register compatible apparel for UET armor traits.
    /// </summary>
    public static class ArmorTraitCompInjector
    {
        public static void Inject()
        {
            foreach (ThingDef def in
                DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.apparel == null)
                    continue;

                // Use the equipment eligibility system
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
                    continue;

                def.comps.Add(new CompProperties_ArmorTraits());
            }
        }
    }
}

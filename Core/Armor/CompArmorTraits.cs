
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class CompProperties_ArmorTraits : CompProperties
    {
        public CompProperties_ArmorTraits()
        {
            compClass = typeof(CompArmorTraits);
        }
    }

    public class CompArmorTraits : ThingComp
    {
        private List<ArmorTraitDef> traits =
            new List<ArmorTraitDef>();

        public IReadOnlyList<ArmorTraitDef> Traits
        {
            get { return traits; }
        }

        public bool HasReactiveTrait
        {
            get
            {
                foreach (ArmorTraitDef trait in traits)
                {
                    if (trait != null && trait.reactive)
                        return true;
                }

                return false;
            }
        }

        public bool AddTrait(ArmorTraitDef trait)
        {
            if (trait == null || traits.Contains(trait))
                return false;

            traits.Add(trait);
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(
                ref traits,
                "uetArmorTraits",
                LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit
                && traits == null)
            {
                traits = new List<ArmorTraitDef>();
            }
        }
    }
}

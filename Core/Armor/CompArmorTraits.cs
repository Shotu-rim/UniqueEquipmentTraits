
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public override string CompInspectStringExtra()
        {
            if (traits == null || traits.Count == 0)
                return null;

            return "WeaponTraits".Translate() + ": " +
                traits.Where(t => t != null)
                      .Select(t => t.label)
                      .ToCommaList()
                      .CapitalizeFirst();
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (traits == null || traits.Count == 0)
                yield break;

            List<ArmorTraitDef> validTraits =
                traits.Where(t => t != null).ToList();

            if (validTraits.Count == 0)
                yield break;

            StringBuilder description = new StringBuilder();

            description.AppendLine(
                "Stat_ThingUniqueWeaponTrait_Desc".Translate());
            description.AppendLine();

            foreach (ArmorTraitDef trait in validTraits)
            {
                description.AppendLine(
                    trait.LabelCap.Colorize(ColorLibrary.Yellow));
                description.AppendLine(trait.description);
                description.AppendLine();
            }

            yield return new StatDrawEntry(
                StatCategoryDefOf.BasicsImportant,
                "Stat_ThingUniqueWeaponTrait_Label".Translate(),
                validTraits.Select(t => t.label)
                           .ToCommaList()
                           .CapitalizeFirst(),
                description.ToString(),
                1104);
        }

        public override string CompTipStringExtra()
        {
            if (traits == null || traits.Count == 0)
                return null;

            return "WeaponTraits".Translate() + ": " +
                traits.Where(t => t != null)
                      .Select(t => t.label)
                      .ToCommaList()
                      .CapitalizeFirst();
        }
    }
}

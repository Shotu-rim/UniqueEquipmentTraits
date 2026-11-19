
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Configures the armor trait component for apparel.
    /// Registers CompArmorTraits as the component implementation.
    /// Other mods can use this class when adding UET armor trait support.
    /// </summary>
    public class CompProperties_ArmorTraits : CompProperties
    {
        public CompProperties_ArmorTraits()
        {
            compClass = typeof(CompArmorTraits);
        }
    }

    /// <summary>
    /// Stores one armor trait and its persistent runtime state
    /// on an individual apparel item.
    /// Other mods can use AddTrait(), Traits and State
    /// to integrate custom armor traits.
    /// </summary>
    public class CompArmorTraits : ThingComp
    {
        private List<ArmorTraitDef> traits =
            new List<ArmorTraitDef>();

        private ArmorTraitState state =
            new ArmorTraitState();

        public IReadOnlyList<ArmorTraitDef> Traits
        {
            get { return traits; }
        }

        public ArmorTraitState State
        {
            get { return state; }
        }

        public bool HasReactiveTrait
        {
            get
            {
                foreach (ArmorTraitDef trait in traits)
                {
                    if (trait != null &&
                        trait.activationType ==
                        ArmorTraitActivationType.Reactive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        //public override void CompTickRare()
        //{
        //    base.CompTickRare();

        //    if (state == null ||
        //        traits == null ||
        //        traits.Count == 0)
        //    {
        //        return;
        //    }

        //    Apparel apparel = parent as Apparel;

        //    if (apparel == null)
        //        return;

        //    Pawn wearer = apparel.Wearer;

        //    if (wearer == null || wearer.health == null)
        //        return;

        //    foreach (ArmorTraitDef trait in traits)
        //    {
        //        if (trait == null ||
        //            trait.activationType !=
        //                ArmorTraitActivationType.Reactive)
        //        {
        //            continue;
        //        }

        //        if (state.ActiveEffectEndTick < 0)
        //            continue;

        //        if (state.IsEffectActive)
        //            continue;

        //        ArmorTraitHediffUtility.Remove(
        //            wearer,
        //            apparel,
        //            trait);
        //    }
        //}

        public override void CompTickRare()
        {
            base.CompTickRare();

            Apparel apparel = parent as Apparel;

            if (apparel == null || apparel.Wearer == null)
                return;

        }


        /// <summary>
        /// Checks whether this apparel item can participate
        /// in reactive armor trait selection.
        /// </summary>

        public bool CanActivateReactive()
        {
            if (!HasReactiveTrait || state == null)
                return false;

            ArmorTraitDef trait = null;

            foreach (ArmorTraitDef candidate in traits)
            {
                if (candidate != null &&
                    candidate.activationType ==
                    ArmorTraitActivationType.Reactive)
                {
                    trait = candidate;
                    break;
                }
            }

            if (trait == null)
                return false;

            if (state.IsEffectActive)
            {
                if (trait.blocksReactivationWhileActive)
                    return false;
            }
            else if (state.IsOnCooldown)
            {
                return false;
            }

            if (!state.HasAvailableCharge)
                return false;

            return true;
        }



        public bool AddTrait(ArmorTraitDef trait)
        {
            if (trait == null ||
                traits == null ||
                traits.Count > 0)
            {
                return false;
            }

            if (parent == null ||
                !ArmorTraitCompatibility.CanApply(
                    parent, trait))
            {
                return false;
            }

            traits.Add(trait);

            state = new ArmorTraitState();

            QualityCategory quality;

            if (parent.TryGetQuality(out quality))
            {
                int maximum =
                    ArmorTraitChargeUtility.GetMaxCharges(
                        trait,
                        quality);

                state.ConfigureCharges(maximum, quality);
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(
                ref traits,
                "uetArmorTraits",
                LookMode.Def);

            Scribe_Deep.Look(
                ref state,
                "uetArmorTraitState");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (traits == null)
                    traits = new List<ArmorTraitDef>();

                if (state == null)
                    state = new ArmorTraitState();
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

            StringBuilder description =
                new StringBuilder();

            description.AppendLine(
                "Stat_ThingUniqueWeaponTrait_Desc".Translate());

            description.AppendLine();

            foreach (ArmorTraitDef trait in validTraits)
            {
                description.AppendLine(
                    trait.LabelCap.Colorize(
                        ColorLibrary.Yellow));

                description.AppendLine(
                    trait.description);

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

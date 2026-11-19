
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Identifies the apparel item and armor trait
    /// responsible for a hediff.
    /// </summary>
    public class HediffComp_ArmorTraitSource : HediffComp
    {
        private Apparel sourceApparel;
        private ArmorTraitDef sourceTrait;

        public Apparel SourceApparel
        {
            get { return sourceApparel; }
        }

        public ArmorTraitDef SourceTrait
        {
            get { return sourceTrait; }
        }

        public void SetSource(
            Apparel apparel,
            ArmorTraitDef trait)
        {
            sourceApparel = apparel;
            sourceTrait = trait;
        }

        public bool IsFrom(
            Apparel apparel,
            ArmorTraitDef trait)
        {
            return sourceApparel == apparel &&
                   sourceTrait == trait;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_References.Look(
                ref sourceApparel,
                "uetArmorSourceApparel");

            Scribe_Defs.Look(
                ref sourceTrait,
                "uetArmorSourceTrait");
        }

        public bool ShouldRemove()
        {
            if (sourceApparel == null ||
                sourceTrait == null ||
                Pawn == null)
            {
                return false;
            }

            // Passive traits are not time-limited.
            if (sourceTrait.activationType ==
                ArmorTraitActivationType.Passive)
            {
                return false;
            }

            CompArmorTraits comp =
                sourceApparel.TryGetComp<CompArmorTraits>();

            if (comp == null ||
                comp.State == null)
            {
                return true;
            }

            return !comp.State.IsEffectActive ||
                   sourceApparel.Wearer != Pawn;
        }
        //public override void CompPostTickInterval(
        //    ref float severityAdjustment,
        //    int delta)
        //{
        //    base.CompPostTickInterval(
        //         ref severityAdjustment,
        //         delta);

        //    if (sourceApparel == null ||
        //        sourceTrait == null ||
        //        Pawn == null ||
        //        Pawn.health == null)
        //    {
        //        return;
        //    }

        //    // Passive effects work when worn
        //    if (sourceTrait.activationType ==
        //        ArmorTraitActivationType.Passive)
        //    {
        //        return;
        //    }

        //    CompArmorTraits comp =
        //        sourceApparel.TryGetComp<CompArmorTraits>();

        //    if (comp == null ||
        //        comp.State == null ||
        //        !comp.State.IsEffectActive ||
        //        sourceApparel.Wearer != Pawn)
        //    {
        //        Pawn.health.RemoveHediff(parent);
        //    }
        //}

    }

    // Properties for the armor trait source hediff comp
    public class HediffCompProperties_ArmorTraitSource
        : HediffCompProperties
    {
        public HediffCompProperties_ArmorTraitSource()
        {
            compClass = typeof(HediffComp_ArmorTraitSource);
        }
    }

}

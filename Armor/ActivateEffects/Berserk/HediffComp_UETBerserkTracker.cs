
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class HediffCompProperties_UETBerserkTracker
        : HediffCompProperties
    {
        public HediffCompProperties_UETBerserkTracker()
        {
            compClass = typeof(HediffComp_UETBerserkTracker);
        }
    }

    public class HediffComp_UETBerserkTracker : HediffComp
    {
        public override void CompPostTickInterval(
            ref float severityAdjustment,
            int delta)
        {
            base.CompPostTickInterval(
                ref severityAdjustment,
                delta);

            Pawn pawn = parent.pawn;

            if (pawn == null ||
                pawn.Dead ||
                pawn.mindState == null ||
                pawn.mindState.mentalStateHandler == null ||
                pawn.mindState.mentalStateHandler.CurStateDef !=
                    MentalStateDefOf.Berserk)
            {
                if (pawn != null &&
                    pawn.health != null &&
                    pawn.health.hediffSet.hediffs.Contains(parent))
                {
                    pawn.health.RemoveHediff(parent);
                }
            }
        }
    }
}


using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class StatPart_Crippling : StatPart
    {
        public override void TransformValue(
            StatRequest req,
            ref float val)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null || pawn.health == null)
                return;

            Hediff_Crippling crippling =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(
                        DefDatabase<HediffDef>
                            .GetNamedSilentFail("UET_Crippling"))
                    as Hediff_Crippling;

            if (crippling == null)
                return;

            val *= 1f - crippling.Slowdown;
        }

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null || pawn.health == null)
                return null;

            Hediff_Crippling crippling =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(
                        DefDatabase<HediffDef>
                            .GetNamedSilentFail("UET_Crippling"))
                    as Hediff_Crippling;

            if (crippling == null)
                return null;

            return "Crippling: -" +
                (crippling.Slowdown * 100f).ToString("0.#") +
                "%";
        }
    }
}

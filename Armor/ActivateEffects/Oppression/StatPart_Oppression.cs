
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class StatPart_Oppression : StatPart
    {
        private static Hediff_Oppression GetOppression(
            StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null || pawn.health == null)
                return null;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Oppression");

            if (def == null)
                return null;

            return pawn.health.hediffSet
                .GetFirstHediffOfDef(def)
                as Hediff_Oppression;
        }

        public override void TransformValue(
            StatRequest req,
            ref float val)
        {
            Hediff_Oppression oppression =
                GetOppression(req);

            if (oppression == null)
                return;

            val *= 1f - oppression.Slowdown;
        }

        public override string ExplanationPart(
            StatRequest req)
        {
            Hediff_Oppression oppression =
                GetOppression(req);

            if (oppression == null)
                return null;

            return "Oppression: -" +
                (oppression.Slowdown * 100f).ToString("0.#") +
                "%";
        }
    }
}

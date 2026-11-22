
using RimWorld;
using UniqueEquipmentTraits.Effects;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class StatPart_Overdrive : StatPart
    {
        public override void TransformValue(
            StatRequest req,
            ref float val)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null || pawn.health == null)
                return;

            Hediff_Overdrive overdrive =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(
                        DefDatabase<HediffDef>.GetNamed(
                            "UET_Overdrive"))
                    as Hediff_Overdrive;

            if (overdrive == null ||
                overdrive.SpeedBonus <= 0f)
            {
                return;
            }

            val *= 1f + overdrive.SpeedBonus;
        }

        public override string ExplanationPart(
            StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null || pawn.health == null)
                return null;

            HediffDef def =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_Overdrive");

            if (def == null)
                return null;

            Hediff_Overdrive overdrive =
                pawn.health.hediffSet
                    .GetFirstHediffOfDef(def)
                    as Hediff_Overdrive;

            if (overdrive == null ||
                overdrive.SpeedBonus <= 0f)
            {
                return null;
            }

            return "Overdrive: +" +
                (overdrive.SpeedBonus * 100f).ToString("0.##") +
                "%";
        }
    }
}

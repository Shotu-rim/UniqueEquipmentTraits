
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    // Productivity skill
    public class StatPart_Productivity : StatPart
    {
        public override void TransformValue(
            StatRequest req,
            ref float val)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null)
                return;

            float bonus =
                ArmorTraitProductivityUtility
                    .GetWorkSpeedBonus(pawn);

            if (bonus > 0f)
                val *= 1f + bonus;
        }

        public override string ExplanationPart(
            StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null)
                return null;

            float bonus =
                ArmorTraitProductivityUtility
                    .GetWorkSpeedBonus(pawn);

            if (bonus <= 0f)
                return null;

            return "Productivity: +" +
                (bonus * 100f).ToString("0.##") +
                "%";
        }
    }
}


using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class StatPart_Hasted : StatPart
    {
        public override void TransformValue(
            StatRequest req,
            ref float val)
        {
            Pawn pawn = req.Thing as Pawn;

            if (pawn == null)
                return;

            float bonus =
                ArmorTraitMoveSpeedUtility
                    .GetMoveSpeedBonus(pawn);

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
                ArmorTraitMoveSpeedUtility
                    .GetMoveSpeedBonus(pawn);

            if (bonus <= 0f)
                return null;

            return "Hasted: +" +
                (bonus * 100f).ToString("0.##") +
                "%";
        }
    }
}

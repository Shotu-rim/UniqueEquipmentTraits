
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_Default : ArmorTraitWorker
    {
        public override void OnActivated(
            ArmorTraitActivationContext context)
        {
            Log.Message(
                "[UET ARMOR STAGE4E] EXECUTED" +
                " trait=" + context.Trait.defName +
                " apparel=" + context.Apparel.def.defName +
                " attacker=" + PawnName(context.Attacker) +
                " defender=" + PawnName(context.Defender) +
                " part=" +
                (context.HitPart != null
                    ? context.HitPart.Label
                    : "null") +
                " stack=" + context.StackNumber);

            Log.Message(
                "[UET ARMOR STAGE4F] DAMAGE" +
                " type=" +
                (context.DamageInfo.Def != null
                    ? context.DamageInfo.Def.defName
                    : "null") +
                " amount=" +
                context.DamageInfo.Amount.ToString("F2") +
                " armorPen=" +
                context.DamageInfo.ArmorPenetrationInt.ToString("F2"));

            Log.Message(
    "[UET ARMOR STAGE4G] INJURY" +
    " severity=" +
    context.InjurySeverity.ToString("F2") +
    " damageInfoAmount=" +
    context.DamageInfo.Amount.ToString("F2"));
        }

        private static string PawnName(Pawn pawn)
        {
            return pawn != null ? pawn.LabelShort : "null";
        }
    }
}


using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Temporary worker for testing reactive armor
    /// trait hediff activation and cleanup.
    /// </summary>
    public class ArmorTraitWorker_TestHediff
        : ArmorTraitWorker
    {
        public override void OnActivated(
            ArmorTraitActivationContext context)
        {
            base.OnActivated(context);

            if (context == null ||
                context.Defender == null ||
                context.Apparel == null ||
                context.Trait == null)
            {
                return;
            }

            HediffDef hediffDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_TestArmorEffect");

            if (hediffDef == null)
            {
                Log.Error(
                    "[UET Armor] Test hediff definition not found.");
                return;
            }

            Hediff hediff =
                ArmorTraitHediffUtility.Apply(
                    context.Defender,
                    context.Apparel,
                    context.Trait,
                    hediffDef);

            if (hediff != null)
            {
                Log.Message(
                    "[UET Armor] Reactive test hediff applied: " +
                    context.Defender.LabelShort +
                    " | Armor: " +
                    context.Apparel.LabelShort +
                    " | Stack: " +
                    context.StackNumber);
            }
        }

        public override void OnUnequipped(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait)
        {
            base.OnUnequipped(pawn, apparel, trait);

            Log.Message(
                "[UET Armor] Reactive test cleanup: " +
                pawn.LabelShort);
        }
    }
}

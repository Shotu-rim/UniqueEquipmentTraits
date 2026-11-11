
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class DisarmingEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            Pawn victim = context.Victim;

            if (victim == null ||
                victim.Dead ||
                victim.equipment == null ||
                !victim.Spawned)
            {
                return;
            }

            ThingWithComps weapon = victim.equipment.Primary;

            // Przeciwnik nie ma broni do wytrącenia.
            if (weapon == null)
                return;

            ThingWithComps dropped;

            bool success = victim.equipment.TryDropEquipment(
                weapon,
                out dropped,
                victim.Position,
                forbid: false);

            // Raportujemy wyłącznie udane rozbrojenie.
            if (!success || dropped == null)
                return;

            context.Report(
                context.Attacker.LabelShort +
                " disarmed " +
                victim.LabelShort +
                ", knocking " +
                dropped.LabelShort +
                " to the ground.");
        }
    }
}

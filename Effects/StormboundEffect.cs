
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class StormboundEffect : UETEffect
    {
        public override void Apply(UETEffectContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.Downed)
                return;

            if (victim.RaceProps == null ||
                !victim.RaceProps.IsMechanoid)
                return;

            int stunTicks =
                UETStunUtility.CalculateStormboundTicks(
                    context.Quality,
                    victim.BodySize
                );

            if (stunTicks <= 0)
                return;

            UETStunUtility.ApplyEMPStun(
                context.Attacker,
                victim,
                stunTicks
            );


            context.Report(
                $"{context.Attacker.LabelShort}'s Stormbound discharged an EMP pulse " +
                $"into {victim.LabelShort}, attempting to stun the target " +
                $"for {stunTicks / 60f:F2} seconds before EMP resistance and adaptation.");

        }
    }
}

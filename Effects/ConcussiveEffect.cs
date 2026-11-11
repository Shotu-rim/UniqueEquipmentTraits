
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class ConcussiveEffect : UETEffect
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
                !victim.RaceProps.IsFlesh ||
                victim.RaceProps.IsMechanoid)
                return;

            int stunTicks =
                UETStunUtility.CalculateConcussiveTicks(
                    context.Quality,
                    victim.BodySize
                );

            if (stunTicks <= 0)
                return;

            UETStunUtility.ApplyNerveStun(
                context.Attacker,
                victim,
                stunTicks
            );

            context.Report(
                $"{context.Attacker.LabelShort} delivered a concussive blow to " +
                $"{context.Victim.LabelShort}, attempting to stun them for " +
                $"{stunTicks / 60f:F2} seconds."); 
      
        }
    }
}


using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class JobDriver_UETCastArmorAbility : JobDriver
    {
        private const int CastDurationTicks = 60;

        private Apparel Armor
        {
            get { return job.targetA.Thing as Apparel; }
        }

        private bool IsArmorValid()
        {
            Apparel armor = Armor;

            return pawn != null &&
                   !pawn.Dead &&
                   !pawn.Downed &&
                   pawn.Spawned &&
                   armor != null &&
                   !armor.Destroyed &&
                   armor.Wearer == pawn;
        }

        public override bool TryMakePreToilReservations(
            bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !IsArmorValid());

            Toil cast = ToilMaker.MakeToil(
                "UET_CastArmorAbility");

            cast.initAction = delegate
            {
                pawn.pather.StopDead();
            };

            cast.tickAction = delegate
            {
                pawn.pather.StopDead();
            };

            cast.defaultCompleteMode = ToilCompleteMode.Delay;
            cast.defaultDuration = CastDurationTicks;

            yield return cast;

            Toil activate = ToilMaker.MakeToil(
                "UET_ActivateArmorAbility");

            activate.initAction = delegate
            {
                if (!IsArmorValid())
                    return;


                bool success =
                    ArmorTraitActivationUtility.TryActivate(
                        pawn,
                        Armor,
                        job.targetB);


                if (success)
                {
                    ArmorTraitSoundUtility.PlayActivationSound(
                        pawn);
                }
            };

            activate.defaultCompleteMode =
                ToilCompleteMode.Instant;

            yield return activate;
        }
    }
}

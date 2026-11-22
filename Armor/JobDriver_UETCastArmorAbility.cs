
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

                LocalTargetInfo target = job.targetB;

                if (target.IsValid)
                {
                    GenDraw.DrawTargetHighlight(target);
                }
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

                Apparel armor = Armor;

                bool success =
                    ArmorTraitActivationUtility.TryActivate(
                        pawn,
                        armor,
                        job.targetB);

                if (!success)
                    return;

                ArmorTraitSoundUtility.PlayActivationSound(
                    pawn);

                CompArmorTraits comp =
                    armor.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    comp.Traits == null ||
                    comp.Traits.Count == 0)
                    return;

                string abilityName =
                    comp.Traits[0].label;

                if (comp.Traits[0].defName ==
                    "UET_NeuralDeafness")
                {
                    abilityName = "Silence";
                }

                Messages.Message(
                    "UET: " +
                    pawn.LabelShort +
                    " used " +
                    abilityName +
                    ".",
                    MessageTypeDefOf.NeutralEvent,
                    false);
            };

            activate.defaultCompleteMode =
                ToilCompleteMode.Instant;

            yield return activate;
        }
    }
}

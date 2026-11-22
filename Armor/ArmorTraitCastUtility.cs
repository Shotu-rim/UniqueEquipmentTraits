
using RimWorld;
using Verse;
using Verse.AI;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitCastUtility
    {
        public const int CastDurationTicks = 60;

        public static bool TryStartCast(
            Pawn caster,
            Apparel apparel,
            ArmorTraitDef trait)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                caster.jobs == null ||
                apparel == null ||
                apparel.Destroyed ||
                apparel.Wearer != caster ||
                trait == null)
            {
                return false;
            }

            JobDef jobDef =
                DefDatabase<JobDef>.GetNamedSilentFail(
                    "UET_CastArmorAbility");

            if (jobDef == null)
            {
                Log.Error(
                    "[UET] Missing JobDef: UET_CastArmorAbility");

                return false;
            }

            Job job = JobMaker.MakeJob(jobDef, apparel);

            // The trait is identified by its Def.
            job.count = 1;

            caster.jobs.TryTakeOrderedJob(job);

            return caster.CurJob == job;
        }

        public static bool TryStartTargetedCast(
            Pawn caster,
            Apparel apparel,
            ArmorTraitDef trait,
            IntVec3 targetCell)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                caster.jobs == null ||
                apparel == null ||
                apparel.Destroyed ||
                apparel.Wearer != caster ||
                trait == null ||
                !targetCell.IsValid ||
                !targetCell.InBounds(caster.Map))
            {
                return false;
            }

            JobDef jobDef =
                DefDatabase<JobDef>.GetNamedSilentFail(
                    "UET_CastArmorAbility");

            if (jobDef == null)
            {
                Log.Error(
                    "[UET] Missing JobDef: UET_CastArmorAbility");

                return false;
            }

            // Target A = armor
            // Target B = selected map cell
            Job job = JobMaker.MakeJob(
                jobDef,
                apparel,
                new LocalTargetInfo(targetCell));

            caster.jobs.TryTakeOrderedJob(job);

            return caster.CurJob == job;
        }

        public static bool TryStartTargetedCast(
            Pawn caster,
            Apparel apparel,
            ArmorTraitDef trait,
            Pawn targetPawn)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                caster.jobs == null ||
                apparel == null ||
                apparel.Destroyed ||
                apparel.Wearer != caster ||
                trait == null ||
                targetPawn == null ||
                targetPawn.Dead ||
                !targetPawn.Spawned ||
                targetPawn.Map != caster.Map)
            {
                return false;
            }

            JobDef jobDef =
                DefDatabase<JobDef>.GetNamedSilentFail(
                    "UET_CastArmorAbility");

            if (jobDef == null)
            {
                Log.Error(
                    "[UET] Missing JobDef: UET_CastArmorAbility");

                return false;
            }

            // Target A = armor
            // Target B = selected pawn
            Job job = JobMaker.MakeJob(
                jobDef,
                apparel,
                new LocalTargetInfo(targetPawn));

            caster.jobs.TryTakeOrderedJob(job);

            return caster.CurJob == job;
        }


    }
}

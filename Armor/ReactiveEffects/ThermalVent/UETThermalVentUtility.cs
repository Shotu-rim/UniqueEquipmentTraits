
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class UETThermalVentUtility
    {
        private const float Radius = 2f;
        private const int MaxHitParts = 3;

        internal static bool HasTargets(Pawn source)
        {
            return GetTargets(source).Count > 0;
        }

        internal static int Emit(
            Pawn source,
            QualityCategory quality)
        {
            List<Pawn> targets = GetTargets(source);

            if (targets.Count == 0)
                return 0;

            int damagePool =
                quality == QualityCategory.Legendary
                    ? 8
                    : 4;

            int affectedTargets = 0;

            UETThermalVentGuard.Begin();

            try
            {
                foreach (Pawn target in targets)
                {
                    if (target == null ||
                        target.Dead ||
                        !target.Spawned)
                    {
                        continue;
                    }

                    List<BodyPartRecord> parts =
                        GetTargetParts(target);

                    if (parts.Count == 0)
                        continue;

                    parts.Shuffle();

                    int hitCount = Math.Min(
                        MaxHitParts,
                        Math.Min(parts.Count, damagePool));

                    int[] allocations =
                        DistributeDamage(damagePool, hitCount);

                    bool attacked = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        if (target.Dead || !target.Spawned)
                            break;

                        DamageInfo damage = new DamageInfo(
                            DamageDefOf.Burn,
                            allocations[i],
                            instigator: source,
                            hitPart: parts[i]);

                        target.TakeDamage(damage);
                        attacked = true;
                    }

                    if (attacked)
                        affectedTargets++;
                }
            }
            finally
            {
                UETThermalVentGuard.End();
            }

            return affectedTargets;
        }

        private static List<Pawn> GetTargets(Pawn source)
        {
            List<Pawn> targets = new List<Pawn>();

            if (source == null ||
                !source.Spawned ||
                source.Map == null)
            {
                return targets;
            }

            foreach (Pawn pawn in source.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn == null ||
                    pawn == source ||
                    pawn.Dead)
                {
                    continue;
                }

                if ((pawn.Position - source.Position)
                    .LengthHorizontalSquared > Radius * Radius)
                {
                    continue;
                }

                targets.Add(pawn);
            }

            return targets;
        }

        private static List<BodyPartRecord> GetTargetParts(
            Pawn target)
        {
            List<BodyPartRecord> parts =
                new List<BodyPartRecord>();

            if (target.health == null ||
                target.RaceProps == null ||
                target.RaceProps.body == null)
            {
                return parts;
            }

            foreach (BodyPartRecord part
                in target.RaceProps.body.AllParts)
            {
                if (part == null ||
                    part.depth != BodyPartDepth.Outside ||
                    target.health.hediffSet.PartIsMissing(part))
                {
                    continue;
                }

                parts.Add(part);
            }

            return parts;
        }

        private static int[] DistributeDamage(
            int total,
            int count)
        {
            int[] allocations = new int[count];

            if (count <= 0)
                return allocations;

            for (int i = 0; i < count; i++)
                allocations[i] = 1;

            int remaining = total - count;

            while (remaining > 0)
            {
                int index = Rand.Range(0, count);

                allocations[index]++;
                remaining--;
            }

            return allocations;
        }
    }
}

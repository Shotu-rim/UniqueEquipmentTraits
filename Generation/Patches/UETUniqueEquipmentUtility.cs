using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETUniqueEquipmentUtility
    {
        private const int MaxAttempts = 10;

        public static ThingWithComps Assign(
            Pawn pawn,
            List<Thing> initialRewards,
            ThingSetMakerParams parms)
        {
            List<Thing> rewards = initialRewards;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (attempt > 0)
                {
                    rewards = ThingSetMakerDefOf
                        .Reward_UniqueWeapon.root.Generate(parms);
                }

                if (rewards == null)
                    continue;

                ThingWithComps assigned = null;

                foreach (Thing reward in rewards)
                {
                    if (assigned == null &&
                        reward is ThingWithComps equipment &&
                        TryAssign(pawn, equipment))
                    {
                        assigned = equipment;
                        continue;
                    }

                    if (reward != null &&
                        reward != assigned &&
                        !reward.Destroyed)
                    {
                        reward.Destroy(DestroyMode.Vanish);
                    }
                }

                if (assigned != null)
                    return assigned;
            }

            // Preserve the pawn's original equipment
            return pawn?.equipment?.Primary;
        }

        private static bool TryAssign(
            Pawn pawn,
            ThingWithComps equipment)
        {
            if (pawn == null ||
                pawn.Dead ||
                equipment == null ||
                equipment.Destroyed)
            {
                return false;
            }

            if (equipment is Apparel apparel)
                return TryAssignApparel(pawn, apparel);

            if (equipment.def.IsWeapon)
                return TryAssignWeapon(pawn, equipment);

            return false;
        }

        private static bool TryAssignWeapon(
            Pawn pawn,
            ThingWithComps weapon)
        {
            if (pawn.equipment == null ||
                weapon.TryGetComp<CompEquippable>() == null)
            {
                return false;
            }

            // Preserve vanilla weapon replacement behavior
            pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
            pawn.equipment.AddEquipment(weapon);

            return pawn.equipment.Contains(weapon);
        }

        private static bool TryAssignApparel(
            Pawn pawn,
            Apparel apparel)
        {
            if (pawn.apparel == null)
                return false;

            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                return false;

            if (!apparel.PawnCanWear(pawn, ignoreGender: true))
                return false;

            if (CompBiocodable.IsBiocoded(apparel) &&
                !CompBiocodable.IsBiocodedFor(apparel, pawn))
            {
                return false;
            }

            List<Apparel> replacedApparel = new List<Apparel>();

            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (!ApparelUtility.CanWearTogether(
                    apparel.def,
                    worn.def,
                    pawn.RaceProps.body))
                {
                    replacedApparel.Add(worn);
                }
            }

            pawn.apparel.Wear(
                apparel,
                dropReplacedApparel: false,
                locked: false);

            if (apparel.Wearer != pawn)
            {
                // Restore apparel removed during a failed assignment
                foreach (Apparel previous in replacedApparel)
                {
                    if (!previous.Destroyed &&
                        previous.Wearer == null)
                    {
                        pawn.apparel.Wear(
                            previous,
                            dropReplacedApparel: false,
                            locked: false);
                    }
                }

                return false;
            }

            // Destroy replaced apparel only after successful assignment
            foreach (Apparel previous in replacedApparel)
            {
                if (!previous.Destroyed &&
                    previous.Wearer == null)
                {
                    previous.Destroy(DestroyMode.Vanish);
                }
            }

            return true;
        }
    }
}

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSystemshockGoodwillUtility
    {
        public static void ApplyPenalties(
            Pawn caster,
            QualityCategory quality,
            IEnumerable<Faction> affectedFactions)
        {
            if (caster == null ||
                affectedFactions == null)
            {
                return;
            }

            Faction playerFaction = Faction.OfPlayer;

            if (playerFaction == null ||
                caster.Faction != playerFaction)
            {
                return;
            }

            int penalty =
                ArmorTraitSystemshockUtility.GetGoodwillPenalty(
                    quality);

            if (penalty >= 0)
                return;

            HashSet<Faction> uniqueFactions =
                new HashSet<Faction>(affectedFactions);

            foreach (Faction faction in uniqueFactions)
            {
                if (faction == null ||
                    faction == playerFaction ||
                    faction.HostileTo(playerFaction))
                {
                    continue;
                }

                faction.TryAffectGoodwillWith(
                    playerFaction,
                    penalty,
                    canSendMessage: true);
            }
        }
    }
}

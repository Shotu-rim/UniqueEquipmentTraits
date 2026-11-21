
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitKindleUtility
    {
        private const int CooldownTicks = 300;
        private const float FireSize = 0.5f;

        public static float GetRange(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Masterwork:
                    return 15f;

                case QualityCategory.Legendary:
                    return 20f;

                default:
                    return 0f;
            }
        }

        public static int GetCooldownTicks()
        {
            return CooldownTicks;
        }

        public static int GetGoodwillPenalty(
            QualityCategory quality)
        {
            return ArmorTraitBerserkUtility.GetGoodwillPenalty(
                quality);
        }

        public static bool CanIgnite(
            Pawn caster,
            IntVec3 cell,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned ||
                caster.Map == null ||
                state == null ||
                state.IsEffectActive ||
                state.IsOnCooldown ||
                !state.UsesCharges ||
                !state.HasAvailableCharge)
            {
                return false;
            }

            float range = GetRange(quality);

            if (range <= 0f ||
                !cell.InBounds(caster.Map) ||
                !caster.Position.InHorDistOf(cell, range))
            {
                return false;
            }

            return FireUtility.ChanceToStartFireIn(
                cell,
                caster.Map) > 0f;
        }

        public static bool TryIgnite(
            Pawn caster,
            IntVec3 cell,
            QualityCategory quality,
            ArmorTraitState state)
        {
            if (!CanIgnite(caster, cell, quality, state))
                return false;

            Map map = caster.Map;

            Faction playerFaction = Faction.OfPlayer;

            HashSet<Faction> affectedFactions =
                new HashSet<Faction>();

            if (caster.Faction == playerFaction &&
                playerFaction != null)
            {
                List<Thing> things = cell.GetThingList(map);

                foreach (Thing thing in things)
                {
                    Faction faction = thing.Faction;

                    if (faction == null ||
                        faction == playerFaction ||
                        faction.HostileTo(playerFaction))
                    {
                        continue;
                    }

                    affectedFactions.Add(faction);
                }
            }

            bool started = FireUtility.TryStartFireIn(
                cell,
                map,
                FireSize,
                caster);

            if (!started)
                return false;

            if (!state.TryConsumeCharge())
                return false;

            state.StartCooldown(CooldownTicks);

            int penalty = GetGoodwillPenalty(quality);

            if (penalty != 0)
            {
                foreach (Faction faction in affectedFactions)
                {
                    faction.TryAffectGoodwillWith(
                        playerFaction,
                        penalty,
                        canSendMessage: true);
                }
            }

            return true;
        }
    }
}

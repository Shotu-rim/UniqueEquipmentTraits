
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public sealed class ArmorTraitSystemshockExplosionData
        : IExposable
    {
        public Pawn Caster;
        public QualityCategory Quality;

        public HashSet<Faction> AffectedFactions =
            new HashSet<Faction>();

        private List<Faction> savedFactions;

        // Required by RimWorld serialization.
        public ArmorTraitSystemshockExplosionData()
        {
        }

        public ArmorTraitSystemshockExplosionData(
            Pawn caster,
            QualityCategory quality)
        {
            Caster = caster;
            Quality = quality;
        }

        public void ExposeData()
        {
            Scribe_References.Look(
                ref Caster,
                "caster");

            Scribe_Values.Look(
                ref Quality,
                "quality",
                QualityCategory.Normal);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                savedFactions =
                    new List<Faction>(AffectedFactions);
            }

            Scribe_Collections.Look(
                ref savedFactions,
                "affectedFactions",
                LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AffectedFactions =
                    savedFactions != null
                        ? new HashSet<Faction>(savedFactions)
                        : new HashSet<Faction>();

                AffectedFactions.Remove(null);
                savedFactions = null;
            }
        }

        public void RegisterAffectedPawn(Pawn pawn)
        {
            if (pawn == null)
                return;

            Faction faction = pawn.Faction;
            Faction playerFaction = Faction.OfPlayer;

            if (faction == null ||
                playerFaction == null ||
                faction == playerFaction ||
                faction.HostileTo(playerFaction))
            {
                return;
            }

            AffectedFactions.Add(faction);
        }

        public void ApplyPenalties()
        {
            ArmorTraitSystemshockGoodwillUtility.ApplyPenalties(
                Caster,
                Quality,
                AffectedFactions);
        }
    }
}


using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    /// <summary>
    /// Represents a UET weapon trait effect entry in RimWorld's native combat log.
    /// Associates the recorded effect description with the attacker and victim,
    /// supports pawn-specific log filtering, and persists entry data in save files.
    /// Other mods can create entries through the public constructors,
    /// although this class is primarily intended for UET combat reporting.
    /// </summary>
    public class UETCombatLogEntry : LogEntry
    {
        private Pawn attacker;
        private Pawn victim;
        private string description;

        public UETCombatLogEntry()
        {
        }

        public UETCombatLogEntry(
            Pawn attacker,
            Pawn victim,
            string description)
        {
            this.attacker = attacker;
            this.victim = victim;
            this.description = description;
        }

        public override bool Concerns(Thing thing)
        {
            return thing != null &&
                (thing == attacker || thing == victim);
        }

        public override IEnumerable<Thing> GetConcerns()
        {
            if (attacker != null)
                yield return attacker;

            if (victim != null && victim != attacker)
                yield return victim;
        }

        protected override string ToGameStringFromPOV_Worker(
            Thing pov,
            bool forceLog)
        {
            return description ?? string.Empty;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(
                ref attacker,
                "uetAttacker",
                saveDestroyedThings: true);

            Scribe_References.Look(
                ref victim,
                "uetVictim",
                saveDestroyedThings: true);

            Scribe_Values.Look(
                ref description,
                "uetDescription",
                "");
        }
    }
}

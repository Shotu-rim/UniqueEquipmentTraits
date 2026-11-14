
using System.Linq;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitHitDiagnosticsRunner : MapComponent
    {
        private bool completed;

        public ArmorTraitHitDiagnosticsRunner(Map map)
            : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (completed)
                return;

            if (!map.IsHashIntervalTick(250))
                return;

            if (!map.IsPlayerHome)
                return;

            Pawn pawn = map.mapPawns.FreeColonistsSpawned
                .FirstOrDefault(p =>
                    p.RaceProps.Humanlike &&
                    p.health?.hediffSet != null &&
                    !p.Dead);

            if (pawn == null)
                return;

            completed = true;

            Log.Message("[UET HIT] Starting diagnostics A+B.");

            // A: safe simulated sequences.
            ArmorTraitHitDiagnostics.RunSequenceTest(pawn);

            // B: observe subsequent real combat.
            ArmorTraitHitSequenceRecorder.Start();

            Log.Message("[UET HIT] Diagnostics initialized.");
        }
    }
}

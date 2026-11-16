
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class DebugAction_UETWorldDiagnostics
    {
        [DebugAction(
            "UET",
            "World Conversion - Print diagnostics",
            allowedGameStates = AllowedGameStates.Playing)]
        public static void PrintDiagnostics()
        {
            Patch_ThingSetMaker_Generate_UETDiagnostics
                .PrintSummary();
        }

        [DebugAction(
            "UET",
            "World Conversion - Reset diagnostics",
            allowedGameStates = AllowedGameStates.Playing)]
        public static void ResetDiagnostics()
        {
            Patch_ThingSetMaker_Generate_UETDiagnostics
                .ResetStatistics();

            Log.Message(
                "[UET][WORLD] Diagnostics reset.");
        }
    }
}

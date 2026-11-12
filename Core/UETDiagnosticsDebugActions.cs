
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    internal static class UETDiagnosticsDebugActions
    {
        [DebugAction(
            "Unique Equipment Traits",
            "Print activation report",
            allowedGameStates = AllowedGameStates.Playing)]
        private static void PrintReport()
        {
            UETActivationDiagnostics.PrintReport();
        }

        [DebugAction(
            "Unique Equipment Traits",
            "Reset activation counters",
            allowedGameStates = AllowedGameStates.Playing)]
        private static void ResetCounters()
        {
            UETActivationDiagnostics.Reset();
        }
    }
}

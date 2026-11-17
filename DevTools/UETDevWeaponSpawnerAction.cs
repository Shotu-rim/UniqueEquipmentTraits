
using LudeonTK;
using Verse;

namespace UniqueEquipmentTraits.DevTools
{
    /// <summary>
    /// Registers a developer debug action that opens the UET equipment
    /// spawner window during gameplay.
    /// Ensures developer mode is enabled and a map is available
    /// before displaying the window.
    /// Intended for internal development and equipment testing.
    /// </summary>
    internal static class UETDevWeaponSpawnerAction
    {
        [DebugAction(
            "Unique Equipment Traits",
            "Open UET Equipment Spawner",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void OpenSpawner()
        {
            if (!Prefs.DevMode || Find.CurrentMap == null)
                return;

            Find.WindowStack.Add(
                new UETDevWeaponSpawnerWindow());
        }
    }
}

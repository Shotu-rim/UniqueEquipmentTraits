
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.DevTools
{
    internal static class UETDevWeaponSpawnerAction
    {
        [DebugAction(
            "Unique Equipment Traits",
            "Open UET Weapon Spawner",
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

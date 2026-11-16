
using LudeonTK;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor.Naming;
using Verse;

namespace UniqueEquipmentTraits.DevTools
{
    public static class UETPawnGenerationTest
    {
        [DebugAction(
            "UET",
            "Test Odyssey pawn generation",
            allowedGameStates = AllowedGameStates.Playing)]
        private static void TestPawnGeneration()
        {
            ArmorNameUtility.TestOdysseyPawnGeneration();
        }
    }
}


using System;
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.DevTools
{
    /// <summary>
    /// Provides a developer action for testing Odyssey-style
    /// random pawn name generation.
    /// Reports the generated name or any errors to the debug log.
    /// Intended for development and diagnostics only.
    /// </summary>
    public static class UETPawnGenerationTest
    {
        [DebugAction(
            "UET",
            "Test Odyssey pawn generation",
            allowedGameStates = AllowedGameStates.Playing)]
        private static void TestPawnGeneration()
        {
            Log.Message("[UET PAWN TEST] Starting.");

            try
            {
                TaleData_Pawn data =
                    TaleData_Pawn.GenerateRandom(humanLike: true);

                if (data == null || data.name == null)
                {
                    Log.Warning(
                        "[UET PAWN TEST] No pawn name generated.");
                    return;
                }

                Log.Message(
                    "[UET PAWN TEST] Success: " +
                    data.name.ToStringFull);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "[UET PAWN TEST] Failed: " + ex);
            }
        }
    }
}

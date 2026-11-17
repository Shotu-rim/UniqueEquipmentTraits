
using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits
{
    /// <summary>
    /// Integrates natural neurotoxic recovery with the pawn health tick system.
    /// Triggers recovery processing at 600-tick intervals, using the
    /// existing NeurotoxicUtility recovery mechanics.
    /// </summary>
    [HarmonyPatch(
        typeof(Pawn_HealthTracker),
        nameof(Pawn_HealthTracker.HealthTickInterval))]
    public static class Patch_NeurotoxicRecovery
    {
        public static void Postfix(
            Pawn_HealthTracker __instance,
            int delta)
        {
            Pawn pawn = __instance.hediffSet.pawn;

            if (pawn == null)
                return;

            if (!pawn.IsHashIntervalTick(600, delta))
                return;

            NeurotoxicUtility.ProcessNaturalRecovery(pawn);
        }
    }
}

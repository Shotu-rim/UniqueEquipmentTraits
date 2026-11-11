
using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits
{
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


using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch]
    internal static class Patch_ArmorTraitRegeneration
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_HealthTracker),
            "HealthTick")]
        private static void HealthTickPostfix(
            Pawn_HealthTracker __instance)
        {
            Advance(__instance, 1);
        }

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(Pawn_HealthTracker),
            "HealthTickInterval")]
        private static void HealthTickIntervalPostfix(
            Pawn_HealthTracker __instance,
            int delta)
        {
            Advance(__instance, delta);
        }

        private static void Advance(
            Pawn_HealthTracker health,
            int delta)
        {
            if (health == null || delta <= 0)
                return;

            Pawn pawn = Traverse.Create(health)
                .Field("pawn")
                .GetValue<Pawn>();

            if (pawn == null || pawn.Dead)
                return;

            if (!pawn.IsHashIntervalTick(60, delta))
                return;

            ArmorTraitRegenerationTracker tracker =
                Current.Game?.GetComponent<
                    ArmorTraitRegenerationTracker>();

            if (tracker == null)
                return;

            tracker.Advance(pawn, delta);
        }
    }
}

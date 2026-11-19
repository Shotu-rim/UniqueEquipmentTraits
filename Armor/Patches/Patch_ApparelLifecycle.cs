
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Tracks apparel changes and forwards lifecycle
    /// notifications to UETEquipmentStateManager.
    /// </summary>
    public static class Patch_ApparelLifecycle
    {
        private static List<Apparel> Snapshot(
            Pawn_ApparelTracker tracker)
        {
            if (tracker == null)
                return null;

            return new List<Apparel>(
                tracker.WornApparel);
        }

        private static void NotifyRemoved(
            Pawn_ApparelTracker tracker,
            List<Apparel> before)
        {
            if (tracker == null ||
                tracker.pawn == null ||
                before == null)
                return;

            foreach (Apparel apparel in before)
            {
                if (!tracker.WornApparel.Contains(apparel))
                {
                    UETEquipmentStateManager
                        .NotifyApparelRemoved(
                            tracker.pawn,
                            apparel);
                }
            }
        }

        [HarmonyPatch(
            typeof(Pawn_ApparelTracker),
            nameof(Pawn_ApparelTracker.Wear),
            new System.Type[]
            {
                typeof(Apparel),
                typeof(bool),
                typeof(bool)
            })]
        public static class WearPatch
        {
            [HarmonyPrefix]
            public static void Prefix(
                Pawn_ApparelTracker __instance,
                out List<Apparel> __state)
            {
                __state = Snapshot(__instance);
            }

            [HarmonyPostfix]
            public static void Postfix(
                Pawn_ApparelTracker __instance,
                Apparel newApparel,
                List<Apparel> __state)
            {
                if (__instance == null ||
                    __instance.pawn == null ||
                    __state == null)
                    return;

                if (!__instance.pawn.Dead &&
                    !__state.Contains(newApparel) &&
                    __instance.WornApparel.Contains(newApparel))
                {
                    UETEquipmentStateManager
                        .NotifyApparelAdded(
                            __instance.pawn,
                            newApparel);
                }
            }
        }

        [HarmonyPatch(
            typeof(Pawn_ApparelTracker),
            nameof(Pawn_ApparelTracker.Remove),
            new System.Type[]
            {
                typeof(Apparel)
            })]
        public static class RemovePatch
        {
            [HarmonyPrefix]
            public static void Prefix(
                Pawn_ApparelTracker __instance,
                Apparel ap,
                out bool __state)
            {
                __state =
                    __instance != null &&
                    ap != null &&
                    __instance.WornApparel.Contains(ap);
            }

            [HarmonyPostfix]
            public static void Postfix(
                Pawn_ApparelTracker __instance,
                Apparel ap,
                bool __state)
            {
                if (!__state ||
                    __instance == null ||
                    __instance.pawn == null)
                    return;

                if (!__instance.WornApparel.Contains(ap))
                {
                    UETEquipmentStateManager
                        .NotifyApparelRemoved(
                            __instance.pawn,
                            ap);
                }
            }
        }

        [HarmonyPatch(
            typeof(Pawn_ApparelTracker),
            nameof(Pawn_ApparelTracker.TryDrop),
            new System.Type[]
    {
            typeof(Apparel),
            typeof(Apparel),
            typeof(IntVec3),
            typeof(bool)
    },
        new ArgumentType[]
    {
            ArgumentType.Normal,
            ArgumentType.Out,
            ArgumentType.Normal,
            ArgumentType.Normal
    })]
        public static class TryDropPatch
        {
            [HarmonyPrefix]
            public static void Prefix(
                Pawn_ApparelTracker __instance,
                Apparel ap,
                out bool __state)
            {
                __state =
                    __instance != null &&
                    ap != null &&
                    __instance.WornApparel.Contains(ap);
            }

            [HarmonyPostfix]
            public static void Postfix(
                Pawn_ApparelTracker __instance,
                Apparel ap,
                bool __state)
            {
                if (!__state ||
                    __instance == null ||
                    __instance.pawn == null)
                    return;

                if (!__instance.WornApparel.Contains(ap))
                {
                    UETEquipmentStateManager
                        .NotifyApparelRemoved(
                            __instance.pawn,
                            ap);
                }
            }
        }

        [HarmonyPatch(
            typeof(Pawn_ApparelTracker),
            nameof(Pawn_ApparelTracker.DestroyAll),
            new System.Type[]
            {
                typeof(DestroyMode)
            })]
        public static class DestroyAllPatch
        {
            [HarmonyPrefix]
            public static void Prefix(
                Pawn_ApparelTracker __instance,
                out List<Apparel> __state)
            {
                __state = Snapshot(__instance);
            }

            [HarmonyPostfix]
            public static void Postfix(
                Pawn_ApparelTracker __instance,
                List<Apparel> __state)
            {
                NotifyRemoved(__instance, __state);
            }
        }
    }
}

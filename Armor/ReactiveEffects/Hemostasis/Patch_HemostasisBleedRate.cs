
using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch]
    public static class Patch_HemostasisBleedRate
    {
        [HarmonyPatch(
            typeof(Hediff_Injury),
            "BleedRate",
            MethodType.Getter)]
        [HarmonyPostfix]
        public static void InjuryPostfix(
            Hediff_Injury __instance,
            ref float __result)
        {
            if (__result <= 0f)
                return;

            HediffComp_Hemostasis comp =
                __instance.TryGetComp<HediffComp_Hemostasis>();

            if (comp != null && comp.BleedingStopped)
                __result = 0f;
        }

        [HarmonyPatch(
            typeof(Hediff_MissingPart),
            "BleedRate",
            MethodType.Getter)]
        [HarmonyPostfix]
        public static void MissingPartPostfix(
            Hediff_MissingPart __instance,
            ref float __result)
        {
            if (__result <= 0f)
                return;

            HediffComp_Hemostasis comp =
                __instance.TryGetComp<HediffComp_Hemostasis>();

            if (comp != null && comp.BleedingStopped)
                __result = 0f;
        }
    }
}

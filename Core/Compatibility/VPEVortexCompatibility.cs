
using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class VPEVortexCompatibility
    {
        public static void Apply(Harmony harmony)
        {
            Type hediffType = AccessTools.TypeByName(
                "VanillaPsycastsExpanded.Staticlord.Hediff_Vortexed");

            if (hediffType == null)
                return;

            MethodInfo getter = AccessTools.PropertyGetter(
                hediffType,
                "CurStage");

            MethodInfo postfix = AccessTools.Method(
                typeof(VPEVortexCompatibility),
                nameof(CurStagePostfix));

            if (getter == null || postfix == null)
            {
                Log.Warning(
                    "[UET] VPE Vortex compatibility skipped: " +
                    "required methods not found.");
                return;
            }

            harmony.Patch(
                getter,
                postfix: new HarmonyMethod(postfix));

            Log.Message(
                "[UET] VPE Vortex Electricity Immunity patch installed.");
        }

        public static void CurStagePostfix(
            Hediff __instance,
            ref HediffStage __result)
        {
            if (__instance == null ||
                __instance.pawn == null ||
                __result == null ||
                __result.capMods == null)
            {
                return;
            }

            // VPE returns a shared base stage when Vortex is null.
            // Never modify that shared stage.
            FieldInfo vortexField = AccessTools.Field(
                __instance.GetType(),
                "Vortex");

            if (vortexField == null ||
                vortexField.GetValue(__instance) == null)
            {
                return;
            }

            float immunity =
                ArmorTraitImmunityUtility.GetBestImmunity(
                    __instance.pawn,
                    "UET_ElectricityImmunity");

            if (immunity <= 0f)
                return;

            foreach (PawnCapacityModifier modifier
                in __result.capMods)
            {
                if (modifier.capacity != PawnCapacityDefOf.Moving &&
                    modifier.capacity != PawnCapacityDefOf.Manipulation)
                {
                    continue;
                }

                // Reduce only the penalty imposed by Vortex.
                modifier.setMax =
                    1f - (1f - modifier.setMax) *
                    (1f - immunity);
            }
        }
    }
}

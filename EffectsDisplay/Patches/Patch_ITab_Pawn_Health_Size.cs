
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Expands the pawn Health tab to reserve space for the future UET Effects panel.
    /// </summary>
    [HarmonyPatch(typeof(ITab_Pawn_Health), MethodType.Constructor)]
    public static class Patch_ITab_Pawn_Health_Size
    {
        private const float TargetWidth = 960f;

        private static readonly FieldInfo SizeField =
            AccessTools.Field(typeof(InspectTabBase), "size");

        public static void Postfix(ITab_Pawn_Health __instance)
        {
            if (SizeField == null)
                return;

            Vector2 currentSize =
                (Vector2)SizeField.GetValue(__instance);

            if (currentSize.x >= TargetWidth)
                return;

            currentSize.x = TargetWidth;
            SizeField.SetValue(__instance, currentSize);
        }
    }
}

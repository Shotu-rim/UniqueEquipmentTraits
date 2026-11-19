
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Preserves the vanilla Health card layout and draws
    /// the UET Effects panel in the additional tab space.
    /// </summary>
    [HarmonyPatch(
        typeof(HealthCardUtility),
        nameof(HealthCardUtility.DrawPawnHealthCard))]
    public static class Patch_HealthCardUtility_DrawPawnHealthCard
    {
        private const float VanillaWidth = 630f;
        private const float TargetWidth = 960f;

        public static void Prefix(
            ref Rect outRect,
            out bool __state)
        {
            __state = outRect.width >= TargetWidth;

            if (__state)
                outRect.width = VanillaWidth;
        }

        public static void Postfix(
            Rect outRect,
            Pawn pawn,
            bool __state)
        {
            if (!__state)
                return;

            Rect effectsRect = new Rect(
                outRect.x + VanillaWidth,
                outRect.y,
                TargetWidth - VanillaWidth,
                outRect.height);

            UETEffectsPanel.Draw(effectsRect, pawn);
        }
    }
}

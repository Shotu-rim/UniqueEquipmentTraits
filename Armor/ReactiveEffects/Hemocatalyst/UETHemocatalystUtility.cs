
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class UETHemocatalystUtility
    {
        internal static bool CanRestoreFocus(Pawn pawn)
        {
            if (pawn == null || pawn.Dead ||
                pawn.psychicEntropy == null)
                return false;

            Pawn_PsychicEntropyTracker tracker =
                pawn.psychicEntropy;

            return tracker.NeedsPsyfocus &&
                   tracker.CurrentPsyfocus >= 0f &&
                   tracker.CurrentPsyfocus < 1f;
        }

        internal static float CalculateFocusGain(
            QualityCategory quality,
            float actualDamage)
        {
            if (actualDamage <= 0f)
                return 0f;

            float strength =
                WeaponTraitQuality.GetEffectStrength(quality);

            if (strength <= 0f)
                return 0f;

            return actualDamage * strength * 2f / 100f;
        }

        internal static float RestoreFocus(
            Pawn pawn,
            float requestedGain)
        {
            if (!CanRestoreFocus(pawn) ||
                requestedGain <= 0f)
                return 0f;

            Pawn_PsychicEntropyTracker tracker =
                pawn.psychicEntropy;

            float before = tracker.CurrentPsyfocus;

            tracker.OffsetPsyfocusDirectly(requestedGain);

            return Mathf.Max(
                0f,
                tracker.CurrentPsyfocus - before);
        }
    }
}

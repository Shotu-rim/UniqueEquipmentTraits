
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Collects equipment effect information for display.
    /// Acts as a shared entry point for weapon and armor
    /// effects. Does not store gameplay state.
    /// </summary>
    public static class UETEffectDisplayManager
    {
        public static List<UETEffectDisplayEntry> GetEffects(
            Pawn pawn)
        {
            List<UETEffectDisplayEntry> entries =
                new List<UETEffectDisplayEntry>();

            if (pawn == null)
                return entries;

            UETWeaponEffectDisplayAdapter.Collect(
                pawn,
                entries);

            UETArmorEffectDisplayAdapter.Collect(
                pawn,
                entries);

            return entries;
        }
    }
}

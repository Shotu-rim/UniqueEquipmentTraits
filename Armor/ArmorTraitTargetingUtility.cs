
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitTargetingUtility
    {
        public static void DrawHoveredTarget()
        {
            if (!Find.Targeter.IsTargeting)
                return;

            Map map = Find.CurrentMap;

            if (map == null)
                return;

            IntVec3 cell = UI.MouseCell();

            if (!cell.IsValid || !cell.InBounds(map))
                return;

            GenDraw.DrawTargetHighlight(
                new LocalTargetInfo(cell));
        }
    }
}

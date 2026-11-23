
using System;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class UETThermalVentGuard
    {
        [ThreadStatic]
        private static int activeDepth;

        internal static bool IsActive
        {
            get { return activeDepth > 0; }
        }

        internal static void Begin()
        {
            activeDepth++;
        }

        internal static void End()
        {
            if (activeDepth > 0)
                activeDepth--;
        }
    }
}


using System;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionContext
    {
        [ThreadStatic]
        private static bool active;

        [ThreadStatic]
        private static QualityCategory sourceQuality;

        public static bool IsActive => active;

        public static QualityCategory SourceQuality =>
            sourceQuality;

        public static void Begin(QualityCategory quality)
        {
            if (active)
            {
                throw new InvalidOperationException(
                    "UET World Conversion context is already active.");
            }

            sourceQuality = quality;
            active = true;
        }

        public static void End()
        {
            active = false;
            sourceQuality = default(QualityCategory);
        }
    }
}

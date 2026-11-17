
using RimWorld;
using System;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Maintains a thread-local context for UET world equipment conversion.
    /// Stores the source item's quality so that conversion patches can
    /// preserve it when creating a unique equipment variant.
    /// Prevents nested contexts and requires explicit cleanup.
    /// </summary>
    public static class UETWorldConversionContext
    {
        [ThreadStatic]
        private static bool active;

        [ThreadStatic]
        private static QualityCategory sourceQuality;

        public static bool IsActive => active;

        public static QualityCategory SourceQuality
        {
            get
            {
                if (!active)
                {
                    throw new InvalidOperationException(
                        "UET World Conversion context is not active.");
                }

                return sourceQuality;
            }
        }

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

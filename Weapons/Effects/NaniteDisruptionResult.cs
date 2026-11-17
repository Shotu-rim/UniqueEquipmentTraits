
using System.Collections.Generic;

namespace UniqueEquipmentTraits.Effects
{
    internal class NaniteDisruptionResult
    {
        internal bool IsMechanoid;

        internal List<NaniteDisruptionPartResult> Parts =
            new List<NaniteDisruptionPartResult>();
    }

    internal class NaniteDisruptionPartResult
    {
        internal string PartName;
        internal float AppliedPercent;
        internal float TotalPercent;
    }
}

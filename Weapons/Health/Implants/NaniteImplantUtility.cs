
using Verse;

namespace UniqueEquipmentTraits.Implants
{
    /// <summary>
    /// Calculates the effective efficiency of artificial body parts
    /// affected by nanite degradation.
    /// Applies accumulated nanite damage as a multiplicative reduction
    /// to the implant's base efficiency, preserving the original value
    /// when no nanite damage component is present.
    /// </summary>
    public static class NaniteImplantUtility
    {
        public static float GetEffectivePartEfficiency(
            Hediff_AddedPart implant)
        {
            float baseEfficiency =
                implant.def.addedPartProps.partEfficiency;

            HediffComp_NaniteDamage comp =
                implant.TryGetComp<HediffComp_NaniteDamage>();

            if (comp == null)
            {
                return baseEfficiency;
            }

            float accumulatedDamage =
                comp.AccumulatedDamage;

            float effectiveEfficiency =
                baseEfficiency * (1f - accumulatedDamage);

            return effectiveEfficiency;
        }
    }
}


using Verse;

namespace UniqueEquipmentTraits.Implants
{
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

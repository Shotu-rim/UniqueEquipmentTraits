
using Verse;
using UniqueEquipmentTraits.Implants;

namespace UniqueEquipmentTraits.Mechanoids
{
    public class Hediff_NaniteMechanicalDegradation
        : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();

            HediffComp_NaniteDamage comp = null;

            if (comps != null)
            {
                foreach (HediffComp candidate in comps)
                {
                    if (candidate is HediffComp_NaniteDamage naniteComp)
                    {
                        comp = naniteComp;
                        break;
                    }
                }
            }

            if (comp == null)
                return;

            float damage = comp.AccumulatedDamage;

            if (Severity != damage)
            {
                Severity = damage;
            }
        }
    }
}

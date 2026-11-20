
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class ArmorTraitRegenerationHealing
    {
        internal static bool TryHealRandomInjury(Pawn pawn)
        {
            if (pawn == null ||
                pawn.health == null ||
                pawn.health.hediffSet == null)
                return false;

            List<Hediff_Injury> eligibleInjuries =
                new List<Hediff_Injury>();

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                Hediff_Injury injury = hediff as Hediff_Injury;

                if (injury == null)
                    continue;

                if (injury.IsPermanent())
                    continue;

                if (injury.Severity <= 0f)
                    continue;

                BodyPartRecord part = injury.Part;

                if (part == null)
                    continue;

                if (pawn.health.hediffSet.PartIsMissing(part))
                    continue;

                if (part.def.destroyableByDamage == false)
                    continue;

                eligibleInjuries.Add(injury);
            }

            if (eligibleInjuries.Count == 0)
                return false;

            Hediff_Injury selected =
                eligibleInjuries.RandomElement();

            selected.Heal(1f);

            return true;
        }
    }
}

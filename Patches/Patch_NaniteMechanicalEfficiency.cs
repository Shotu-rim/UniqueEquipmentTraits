
using HarmonyLib;
using System.Collections.Generic;
using UniqueEquipmentTraits.Implants;
using UniqueEquipmentTraits.Mechanoids;
using Verse;
using static Verse.PawnCapacityUtility;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(
        typeof(PawnCapacityUtility),
        nameof(PawnCapacityUtility.CalculatePartEfficiency))]
    public static class Patch_NaniteMechanicalEfficiency
    {
        [HarmonyPostfix]
        public static void Postfix(
            HediffSet diffSet,
            BodyPartRecord part,
            List<CapacityImpactor> impactors,
            ref float __result)
        {
            if (diffSet == null || part == null)
                return;

            Pawn pawn = diffSet.pawn;

            if (pawn == null ||
                pawn.RaceProps == null ||
                !pawn.RaceProps.IsMechanoid)
                return;

            if (__result <= 0f)
                return;

            foreach (Hediff hediff in diffSet.hediffs)
            {
                if (!(hediff is
                    Hediff_NaniteMechanicalDegradation degradation))
                    continue;

                if (degradation.Part != part)
                    continue;

                HediffComp_NaniteDamage comp =
                    degradation.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null)
                    continue;

                float damage = comp.AccumulatedDamage;

                if (damage <= 0f)
                    continue;

                __result *= 1f - damage;

                // Adding Hediff to Overview
                if (impactors != null)
                {
                    impactors.Add(new CapacityImpactorHediff
                    {
                        hediff = degradation
                    });
                }
            }
        }
    }
}

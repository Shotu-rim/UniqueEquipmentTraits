
using LudeonTK;
using RimWorld;
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Implants
{
    public static class NaniteDebugActions
    {
        [DebugAction(
            "Unique Equipment Traits",
            "Damage implant (30%)",
            actionType = DebugActionType.ToolMapForPawns)]
        public static void DamageImplant(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (!(hediff is Hediff_AddedPart implant))
                {
                    continue;
                }

                HediffComp_NaniteDamage comp =
                    implant.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null)
                {
                    continue;
                }

                float before = comp.AccumulatedDamage;

                comp.ApplyDamage(0.30f);

                Log.Message(
                    $"[UET] TEST: {pawn.LabelShort}, " +
                    $"implant: {implant.def.defName}, " +
                    $"damage: {before:P0} -> " +
                    $"{comp.AccumulatedDamage:P0}"
                );

                return;
            }

            Log.Message(
                $"[UET] TEST: {pawn.LabelShort} " +
                "has no eligible implants."
            );
        }


        [DebugAction(
            "Unique Equipment Traits",
            "Remove implant (test)",
            actionType = DebugActionType.ToolMapForPawns)]
        public static void RemoveImplant(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
            {
                return;
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                if (!(hediff is Hediff_AddedPart implant))
                {
                    continue;
                }

                HediffComp_NaniteDamage comp =
                    implant.TryGetComp<HediffComp_NaniteDamage>();

                if (comp == null)
                {
                    continue;
                }

                string implantName = implant.def.defName;

                pawn.health.RemoveHediff(implant);

                Log.Message(
                    $"[UET] TEST: Removed implant {implantName} " +
                    $"from {pawn.LabelShort}"
                );

                return;
            }

            Log.Message(
                $"[UET] TEST: {pawn.LabelShort} has no eligible implants."
            );
        }



    }
}

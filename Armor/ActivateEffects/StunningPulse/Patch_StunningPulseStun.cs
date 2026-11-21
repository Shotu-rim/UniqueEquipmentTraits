
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(
        typeof(StunHandler),
        "CanBeStunnedByDamage")]
    public static class Patch_StunningPulseStun
    {
        [HarmonyPostfix]
        public static void Postfix(
            StunHandler __instance,
            DamageDef def,
            ref bool __result)
        {
            if (__result ||
                def == null ||
                def.defName != "UET_StunningPulse")
                return;

            if (!def.causeStun)
                return;

            Pawn pawn = __instance.parent as Pawn;

            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed)
                return;

            if (ModsConfig.AnomalyActive &&
                pawn.health != null &&
                pawn.health.hediffSet != null &&
                pawn.health.hediffSet.HasHediff(
                    HediffDefOf.AwokenCorpse))
                return;

            CompStunnable stunnable =
                pawn.TryGetComp<CompStunnable>();

            if (stunnable != null &&
                !stunnable.CanBeStunnedByDamage(def))
                return;

            __result = true;
        }
    }
}


using HarmonyLib;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Records actual body part damage dealt during an active UET
    /// melee attack.
    /// Captures finalized injury damage and its associated body part
    /// and injury definition, while excluding unrelated damage events.
    /// </summary>
    [HarmonyPatch(
        typeof(DamageWorker_AddInjury),
        "FinalizeAndAddInjury",
        new[]
        {
            typeof(Pawn),
            typeof(Hediff_Injury),
            typeof(DamageInfo),
            typeof(DamageWorker.DamageResult)
        })]
    internal static class Patch_DamageWorker_AddInjury
    {
        [HarmonyPostfix]
        private static void Postfix(
            Pawn pawn,
            Hediff_Injury injury,
            DamageInfo dinfo,
            float __result)
        {
            MeleeAttackContext context =
                Patch_Verb_MeleeAttackDamage.CurrentContext;

            if (context == null)
                return;

            if (context.Attacker == null ||
                context.Victim == null)
                return;

            if (pawn != context.Victim)
                return;

            if (dinfo.Instigator != context.Attacker)
                return;

            if (injury == null || injury.Part == null)
                return;

            if (__result <= 0f)
                return;

            context.RegisterPartDamage(
                injury.Part,
                __result,
                injury.def);
        }
    }
}

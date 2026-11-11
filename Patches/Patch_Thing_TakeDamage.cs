
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    internal static class Patch_Thing_TakeDamage
    {
        [HarmonyPostfix]
        private static void Postfix(
            Thing __instance,
            DamageInfo dinfo,
            DamageWorker.DamageResult __result)
        {
            MeleeAttackContext context =
                Patch_Verb_MeleeAttackDamage.CurrentContext;

            if (context == null)
                return;

            if (context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            if (__instance != context.Victim)
                return;

            if (dinfo.Instigator != context.Attacker)
                return;

            if (__result == null ||
                __result.totalDamageDealt <= 0f)
                return;

            // Rejestrujemy rzeczywiste obrażenia.
            context.RegisterDamage(
                __result.totalDamageDealt);

            if (__result.parts != null)
            {
                foreach (BodyPartRecord part in __result.parts)
                {
                    context.RegisterHitPart(part);
                }
            }

            if (__result.hediffs != null)
            {
                foreach (Hediff hediff in __result.hediffs)
                {
                    if (hediff == null ||
                        hediff.Part == null)
                        continue;

                    context.RegisterHitPart(hediff.Part);
                }
            }
        }
    }
}

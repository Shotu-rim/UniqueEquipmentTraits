
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(Thing),
        nameof(Thing.TakeDamage))]
    public static class Patch_ArmorHeatDamageImmunity
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(
            Thing __instance,
            ref DamageInfo dinfo,
            ref DamageWorker.DamageResult __result)
        {
            Apparel apparel = __instance as Apparel;

            if (apparel == null ||
                dinfo.Def == null ||
                dinfo.Amount <= 0f)
            {
                return true;
            }

            string immunityTrait =
                ArmorTraitImmunityDamageUtility.GetImmunityTrait(
                    dinfo.Def);

            if (immunityTrait == null)
                return true;

            float immunity =
                ArmorTraitImmunityUtility.GetItemImmunity(
                    apparel,
                    immunityTrait);

            if (immunity <= 0f)
                return true;

            float remainingDamage =
                dinfo.Amount * (1f - immunity);

            if (remainingDamage <= 0f)
            {
                __result = new DamageWorker.DamageResult();
                return false;
            }

            CompArmorTraits comp =
                apparel.TryGetComp<CompArmorTraits>();

            if (comp == null)
                return true;

            int damageToApply =
                comp.AccumulateImmunityDamage(
                    remainingDamage);

            if (damageToApply <= 0)
            {
                __result = new DamageWorker.DamageResult();
                return false;
            }

            dinfo.SetAmount(damageToApply);

            return true;
        }
    }
}

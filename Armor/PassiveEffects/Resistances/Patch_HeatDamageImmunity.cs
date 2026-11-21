using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    [HarmonyPatch(
        typeof(Pawn),
        nameof(Pawn.PreApplyDamage))]
    public static class Patch_HeatDamageImmunity
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(
            Pawn __instance,
            ref DamageInfo dinfo,
            ref bool absorbed)
        {
            if (__instance == null ||
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
                ArmorTraitImmunityUtility.GetBestImmunity(
                    __instance,
                    immunityTrait);

            if (immunity <= 0f)
                return true;

            // Legendary Electricity Immunity: fully absorb EMP before vanilla processes its effects
            if (dinfo.Def == DamageDefOf.EMP &&
                immunity >= 1f)
            {
                absorbed = true;
                return false;
            }

            dinfo.SetAmount(
                dinfo.Amount * (1f - immunity));

            return true;
        }
    }
}
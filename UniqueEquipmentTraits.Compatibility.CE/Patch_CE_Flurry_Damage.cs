using System.Collections.Generic;
using CombatExtended;
using HarmonyLib;
using UniqueEquipmentTraits.Patches;
using Verse;
using System;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    /// <summary>
    /// Applies the shared Flurry damage multiplier
    /// to Combat Extended melee damage.
    /// </summary>
    [HarmonyPatch(
        typeof(Verb_MeleeAttackCE),
        "DamageInfosToApply",
        new Type[]
        {
            typeof(LocalTargetInfo),
            typeof(bool)
        })]
    internal static class Patch_CE_Flurry_Damage
    {
        [HarmonyPostfix]
        private static void Postfix(
            ref IEnumerable<DamageInfo> __result)
        {
            __result =
                Patch_Flurry_Damage.ApplyDamageMultiplier(
                    __result);
        }
    }
}
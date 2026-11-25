using CombatExtended;
using HarmonyLib;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    [HarmonyPatch(typeof(Verb_MeleeAttackCE), "GetDodgeChance")]
    internal static class Patch_CE_Dodge
    {
        [HarmonyPostfix]
        private static void Postfix(
            Pawn defender,
            ref float __result)
        {
            if (defender == null || defender.Dead)
                return;

            if (defender.stances.stunner.Stunned)
                return;

            float baseDodge = __result;

            __result =
                UETDodgeUtility.ApplyDodgeBonuses(
                    defender,
                    baseDodge);

        }
    }
}
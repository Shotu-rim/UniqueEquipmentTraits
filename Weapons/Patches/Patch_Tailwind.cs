using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Applies UET dodge bonuses while preserving
    /// vanilla restrictions on dodging.
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    internal static class Patch_Tailwind_Dodge
    {
        //[HarmonyPostfix]
        //private static void Postfix(
        //    Verb_MeleeAttack __instance,
        //    LocalTargetInfo target,
        //    ref float __result)
        //{
        //    Pawn defender = target.Pawn;

        //    if (defender == null || defender.Dead)
        //        return;

        //    if (!CanDodge(__instance, defender))
        //        return;

        //    __result =
        //        UETDodgeUtility.ApplyDodgeBonuses(
        //            defender,
        //            __result);
        //}
        [HarmonyPostfix]
        private static void Postfix(
    Verb_MeleeAttack __instance,
    LocalTargetInfo target,
    ref float __result)
        {
            Pawn defender = target.Pawn;

            if (defender == null || defender.Dead)
                return;

            if (!CanDodge(__instance, defender))
            {
                Log.Message(
                    "[UET DODGE] " + defender.LabelShort +
                    " | CanDodge: FALSE" +
                    " | Vanilla: " + __result.ToString("P2"));

                return;
            }

            float baseDodge = __result;

            float armorDodge =
                ArmorTraitDodgeUtility.ApplyDodgeBonus(
                    defender,
                    baseDodge);

            __result =
                UETDodgeUtility.ApplyDodgeBonuses(
                    defender,
                    baseDodge);

            Log.Message(
                "[UET DODGE] " + defender.LabelShort +
                " | CanDodge: TRUE" +
                " | Vanilla: " + baseDodge.ToString("P2") +
                " | After armor: " + armorDodge.ToString("P2") +
                " | Final: " + __result.ToString("P2"));
        }

        private static bool CanDodge(
            Verb_MeleeAttack attack,
            Pawn defender)
        {
            if (Traverse.Create(attack)
                .Field("surpriseAttack")
                .GetValue<bool>())
                return false;

            if (defender.Downed ||
                defender.GetPosture() != PawnPosture.Standing)
                return false;

            if (defender.stances.curStance is Stance_Busy busy &&
                busy.verb != null &&
                !busy.verb.verbProps.IsMeleeAttack)
                return false;

            return true;
        }
    }
}
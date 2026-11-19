
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Applies passive armor dodge bonuses and the active
    /// Tailwind weapon trait bonus to melee dodge chance.
    /// Preserves vanilla restrictions on dodging.
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    internal static class Patch_Tailwind_Dodge
    {
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
                return;

            // Apply passive armor bonuses first
            __result =
                ArmorTraitDodgeUtility.ApplyDodgeBonus(
                    defender,
                    __result);

            // Apply active Tailwind bonus afterward
            ThingWithComps weapon =
                defender.equipment?.Primary;

            CompWeaponTrait comp =
                weapon?.TryGetComp<CompWeaponTrait>();

            if (comp != null &&
                comp.TailwindActiveFor(defender))
            {
                float bonus = comp.TailwindDodgeBonus();

                if (bonus > 0f)
                    __result += bonus;
            }

            __result = UnityEngine.Mathf.Clamp01(__result);
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

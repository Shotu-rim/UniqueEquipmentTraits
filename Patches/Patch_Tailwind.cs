
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
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

            ThingWithComps weapon =
                defender.equipment?.Primary;

            CompWeaponTrait comp =
                weapon?.TryGetComp<CompWeaponTrait>();

            if (comp == null ||
                !comp.TailwindActiveFor(defender))
                return;

            // Preserve vanilla restrictions on dodging.
            if (!CanDodge(__instance, defender))
                return;

            float bonus = comp.TailwindDodgeBonus();

            if (bonus <= 0f)
                return;

            __result = UnityEngine.Mathf.Clamp01(
                __result + bonus);
        }

        private static bool CanDodge(
            Verb_MeleeAttack attack,
            Pawn defender)
        {
            // Surprise attacks cannot be dodged.
            // Other vanilla restrictions are checked below.
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

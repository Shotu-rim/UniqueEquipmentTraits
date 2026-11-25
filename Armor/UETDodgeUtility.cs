using UnityEngine;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class UETDodgeUtility
    {
        public static float ApplyDodgeBonuses(
            Pawn defender,
            float baseDodge)
        {
            if (defender == null || defender.Dead)
                return baseDodge;

            // Apply passive armor bonuses first
            float result =
                ArmorTraitDodgeUtility.ApplyDodgeBonus(
                    defender,
                    baseDodge);

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
                    result += bonus;
            }

            return Mathf.Clamp01(result);
        }
    }
}
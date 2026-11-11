
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Traits;
using UniqueEquipmentTraits.Effects;

namespace UniqueEquipmentTraits.Core
{
    internal static class WeaponTraitActivation
    {
        internal static void ProcessMeleeAttack(
            MeleeAttackContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            if (context.Attacker.Dead ||
                !context.DamageDealt)
                return;

            RimWorld.WeaponTraitDef trait =
                GetUETTrait(context.Weapon);

            if (trait == null)
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            float activationChance =
                WeaponTraitQuality.GetActivationChance(
                    qualityComp.Quality);

            if (activationChance <= 0f ||
                !Rand.Chance(activationChance))
                return;

            float effectStrength =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            UETEffectContext effectContext =
                new UETEffectContext(
                    context.Attacker,
                    context.Victim,
                    context.Weapon,
                    qualityComp.Quality,
                    effectStrength,
                    context.BleedingBeforeAttack,
                    context.HitParts,
                    context.DamageAmount,
                    context.PartDamage,
                    context.PartInjuryDefs);

            UETEffectDispatcher.TryApply(
                trait,
                effectContext);
        }


        private static RimWorld.WeaponTraitDef GetUETTrait(
            ThingWithComps weapon)
        {
            CompUniqueWeapon uniqueComp =
                weapon.TryGetComp<CompUniqueWeapon>();

            if (uniqueComp == null ||
                uniqueComp.TraitsListForReading == null)
                return null;

            foreach (RimWorld.WeaponTraitDef trait
                in uniqueComp.TraitsListForReading)
            {
                if (trait != null &&
                    trait.GetModExtension<UETTraitExtension>() != null)
                {
                    return trait;
                }
            }

            return null;
        }

    }
}


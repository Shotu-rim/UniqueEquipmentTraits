
using RimWorld;
using UniqueEquipmentTraits.Effects;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Handles activation of UET melee weapon traits after an attack.
    /// Processes traits with dedicated activation logic and dispatches
    /// other eligible effects using quality-based activation chances
    /// and effect strength.
    /// </summary>
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

            RimWorld.WeaponTraitDef trait =
                GetUETTrait(context.Weapon);

            if (trait == null)
                return;

            if (trait.defName == "UET_Prismatic")
            {
                PrismaticEffect.Resolve(context);
                return;
            }

            if (trait.defName == "UET_Flurry")
            {
                if (context.DamageDealt && !context.Attacker.Dead)
                {
                    CompWeaponTrait comp =
                        context.Weapon.TryGetComp<CompWeaponTrait>();

                    if (comp == null)
                    {
                        Log.Warning(
                            "[UET Flurry] Missing CompWeaponTrait on weapon: " +
                            context.Weapon.LabelCap);
                    }
                    else
                    {
                        comp.TryActivateFlurry(context.Attacker);
                    }
                }

                return;
            }

            if (trait.defName == "UET_Tailwind")
            {
                if (context.DamageDealt && !context.Attacker.Dead)
                {
                    CompWeaponTrait comp =
                        context.Weapon.TryGetComp<CompWeaponTrait>();

                    if (comp == null)
                    {
                        Log.Warning(
                            "[UET Tailwind] Missing CompWeaponTrait on weapon: " +
                            context.Weapon.LabelCap);
                    }
                    else
                    {
                        comp.TryActivateTailwind(context.Attacker);
                    }
                }

                return;
            }

            if (trait.defName == "UET_Sundering")
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            if (context.Attacker.Dead ||
                !context.DamageDealt)
                return;

            float activationChance =
                WeaponTraitQuality.GetActivationChance(
                    qualityComp.Quality);

            // Nanite Disruptor uses its own activation chances
            if (trait.defName == "UET_NaniteDisruptor")
            {
                switch (qualityComp.Quality)
                {
                    case QualityCategory.Masterwork:
                        activationChance = 0.35f;
                        break;

                    case QualityCategory.Legendary:
                        activationChance = 0.45f;
                        break;
                }
            }

            if (activationChance <= 0f)
                return;

            bool activated = Rand.Chance(activationChance);

            if (!activated)
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

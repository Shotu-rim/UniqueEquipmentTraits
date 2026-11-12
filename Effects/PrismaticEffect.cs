
using RimWorld;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using Verse;
using WeaponTraitDef = RimWorld.WeaponTraitDef;

namespace UniqueEquipmentTraits.Effects
{
    internal static class PrismaticEffect
    {
        private const string PrismaticDefName = "UET_Prismatic";
        private const string MeleeCategory = "UET_Melee";

        internal static WeaponTraitDef RollEffect()
        {
            List<WeaponTraitDef> candidates =
                new List<WeaponTraitDef>();

            foreach (WeaponTraitDef trait in
                DefDatabase<WeaponTraitDef>.AllDefsListForReading)
            {
                if (trait == null)
                    continue;

                // Prismatic cannot select itself.
                if (trait.defName == PrismaticDefName)
                    continue;

                // Only melee traits from the UET category.
                if (trait.weaponCategory == null ||
                    trait.weaponCategory.defName != MeleeCategory)
                    continue;

                // UET extension is the compatibility contract.
                if (trait.GetModExtension<UETTraitExtension>() == null)
                    continue;

                candidates.Add(trait);
            }

            if (candidates.Count == 0)
                return null;

            // Every eligible trait has equal probability.
            return candidates.RandomElement();
        }


        internal static void Prepare(MeleeAttackContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null ||
                context.Attacker.Dead)
                return;

            // Only weapons with Prismatic can use this mechanic.
            if (!HasPrismatic(context.Weapon))
                return;

            // One trait selection per attack.
            if (context.PrismaticSelectedTrait != null)
                return;

            WeaponTraitDef selectedTrait = RollEffect();

            if (selectedTrait == null)
                return;

            context.PrismaticSelectedTrait = selectedTrait;

            // Sundering must be resolved before armor calculations.
            if (selectedTrait.defName != "UET_Sundering")
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            float effectStrength =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            float activationChance =
                UnityEngine.Mathf.Clamp01(effectStrength * 2f);

            context.PrismaticActivationRolled = true;
            context.PrismaticActivated =
                Rand.Chance(activationChance);

            if (!context.PrismaticActivated)
                return;

            context.ArmorPiercingActivated = true;
            context.ArmorPiercingStrength = effectStrength;
        }


        internal static void Resolve(MeleeAttackContext context)
        {
            if (context == null ||
                context.PrismaticSelectedTrait == null)
                return;

            // Sundering was already resolved before armor calculation.
            if (context.PrismaticSelectedTrait.defName == "UET_Sundering")
                return;

            // Other effects require an actual damaging hit.
            if (!context.DamageDealt ||
                context.Attacker == null ||
                context.Attacker.Dead ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            // Prevent a second activation roll.
            if (context.PrismaticActivationRolled)
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            float effectStrength =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);

            float activationChance =
                UnityEngine.Mathf.Clamp01(effectStrength * 2f);

            context.PrismaticActivationRolled = true;
            context.PrismaticActivated =
                Rand.Chance(activationChance);

            if (!context.PrismaticActivated)
                return;

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

            TryExecuteEffect(
                context.PrismaticSelectedTrait,
                effectContext);
        }


        internal static bool TryExecuteEffect(
            WeaponTraitDef selectedTrait,
            UETEffectContext context)
        {
            if (selectedTrait == null || context == null)
                return false;

            UETTraitExtension extension =
                selectedTrait.GetModExtension<UETTraitExtension>();

            if (extension == null)
                return false;

            // Stateful effects use the existing weapon component.
            if (selectedTrait.defName == "UET_Flurry" ||
                selectedTrait.defName == "UET_Tailwind")
            {
                CompWeaponTrait comp =
                    context.Equipment?.TryGetComp<CompWeaponTrait>();

                if (comp == null)
                    return false;

                if (selectedTrait.defName == "UET_Flurry")
                    return comp.ActivatePrismaticFlurry(
                        context.Attacker);

                return comp.ActivatePrismaticTailwind(
                    context.Attacker);
            }

            // Standard effects are executed without another
            // activation roll.
            if (extension.effectClass != null)
                return UETEffectDispatcher.TryApply(
                    selectedTrait,
                    context);

            return false;
        }



        internal static bool HasPrismatic(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;

            CompUniqueWeapon unique =
                weapon.TryGetComp<CompUniqueWeapon>();

            if (unique?.TraitsListForReading == null)
                return false;

            foreach (RimWorld.WeaponTraitDef trait
                in unique.TraitsListForReading)
            {
                if (trait != null &&
                    trait.defName == "UET_Prismatic")
                    return true;
            }

            return false;
        }


    }
}

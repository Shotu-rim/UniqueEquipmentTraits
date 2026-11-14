
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
        private static List<WeaponTraitDef> cachedCandidates;
        private const bool DebugPrismatic = true;

        private static void DebugLog(string message)
        {
            if (DebugPrismatic && Prefs.DevMode)
                Log.Message("[UET Prismatic] " + message);
        }

        internal static WeaponTraitDef RollEffect()
        {
            // Cache traits
            if (cachedCandidates == null)
                cachedCandidates = BuildCandidates();

            if (cachedCandidates.Count == 0)
                return null;

            // Random traits
            return cachedCandidates.RandomElement();
        }

        private static List<WeaponTraitDef> BuildCandidates()
        {
            List<WeaponTraitDef> candidates =
                new List<WeaponTraitDef>();

            foreach (WeaponTraitDef trait in
                DefDatabase<WeaponTraitDef>.AllDefsListForReading)
            {
                if (trait == null)
                    continue;

                // Prismatic cannot select itself
                if (trait.defName == PrismaticDefName)
                    continue;

                // Only UET traits
                if (trait.weaponCategory == null ||
                    trait.weaponCategory.defName != MeleeCategory)
                    continue;

                // Need UET
                if (trait.GetModExtension<UETTraitExtension>() == null)
                    continue;

                candidates.Add(trait);
            }

            return candidates;
        }


        internal static void Prepare(MeleeAttackContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null ||
                context.Attacker.Dead)
                return;

            // Only Prismatic can use
            if (!HasPrismatic(context.Weapon))
                return;

            // Choose trait per attack
            if (context.PrismaticSelectedTrait != null)
                return;


            WeaponTraitDef selectedTrait = RollEffect();

            if (selectedTrait == null)
                return;

            context.PrismaticSelectedTrait = selectedTrait;

            // Sundering before resolve
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

            // Sundering resolved before attack
            if (context.PrismaticSelectedTrait.defName == "UET_Sundering")
                return;

            // Damage required to init
            if (!context.DamageDealt ||
                context.Attacker == null ||
                context.Attacker.Dead ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            // Prevent second activation
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

            bool executed = TryExecuteEffect(
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

            // Use charges
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

            // Effects executed without another roll, Prismatic already did that
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


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
                            "[UET Flurry] Brak CompWeaponTrait na broni: " +
                            context.Weapon.LabelCap);
                    }
                    else
                    {
                        bool flurryActivated =
                            comp.TryActivateFlurry(context.Attacker);

                        if (flurryActivated)
                        {
                            Log.Message(
                                "[UET Flurry] ACTIVATED! " +
                                "Broń: " + context.Weapon.LabelCap +
                                ", Pawn: " + context.Attacker.LabelShort);
                        }
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

            // UET DIAGNOSTICS
            UETActivationDiagnostics.RecordAttack(
                qualityComp.Quality,
                context.DamageDealt);

            if (context.Attacker.Dead ||
                !context.DamageDealt)
                return;

            float activationChance =
                 WeaponTraitQuality.GetActivationChance(
                 qualityComp.Quality);

            // Nanite Disruptor exception
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

            // UET DIAGNOSTICS
            UETActivationDiagnostics.RecordRoll(
                qualityComp.Quality,
                activated);

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

            bool dispatched = UETEffectDispatcher.TryApply(
    trait,
    effectContext);

            // UET DIAGNOSTICS
            if (dispatched)
            {
                UETActivationDiagnostics.RecordDispatch(
                    qualityComp.Quality);
            }
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


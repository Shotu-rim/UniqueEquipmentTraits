
using RimWorld;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    internal class SunderingEffect : UETEffect
    {
        internal static void Prepare(MeleeAttackContext context)
        {
            if (context == null ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            CompUniqueWeapon uniqueComp =
                context.Weapon.TryGetComp<CompUniqueWeapon>();

            if (uniqueComp?.TraitsListForReading == null)
                return;

            bool hasSundering = false;

            foreach (RimWorld.WeaponTraitDef trait
                in uniqueComp.TraitsListForReading)
            {
                if (trait != null &&
                    trait.defName == "UET_Sundering" &&
                    trait.GetModExtension<UETTraitExtension>() != null)
                {
                    hasSundering = true;
                    break;
                }
            }

            if (!hasSundering)
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            float chance =
                WeaponTraitQuality.GetActivationChance(
                    qualityComp.Quality);

            if (qualityComp.Quality == QualityCategory.Masterwork)
                chance = 0.35f;
            else if (qualityComp.Quality == QualityCategory.Legendary)
                chance = 0.45f;

            if (chance <= 0f || !Rand.Chance(chance))
                return;

            context.ArmorPiercingActivated = true;
            context.ArmorPiercingStrength =
                WeaponTraitQuality.GetEffectStrength(
                    qualityComp.Quality);
        }

        public override void Apply(UETEffectContext context)
        {
            // Sundering działa przed obliczeniem pancerza.
            // Nie nakłada dodatkowych efektów po trafieniu.
        }



        internal static void RecordCombatLog(MeleeAttackContext context)
        {
            if (context == null ||
                !context.ArmorPiercingActivated ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                return;

            RimWorld.WeaponTraitDef sundering = null;

            // Sundering selected by Prismatic.
            if (context.PrismaticSelectedTrait != null &&
                context.PrismaticSelectedTrait.defName == "UET_Sundering" &&
                context.PrismaticActivated)
            {
                sundering = context.PrismaticSelectedTrait;
            }
            else
            {
                // Regular Sundering present directly on the weapon.
                CompUniqueWeapon uniqueComp =
                    context.Weapon.TryGetComp<CompUniqueWeapon>();

                if (uniqueComp?.TraitsListForReading == null)
                    return;

                foreach (RimWorld.WeaponTraitDef trait
                    in uniqueComp.TraitsListForReading)
                {
                    if (trait != null &&
                        trait.defName == "UET_Sundering")
                    {
                        sundering = trait;
                        break;
                    }
                }
            }

            if (sundering == null)
                return;

            CompQuality qualityComp =
                context.Weapon.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            UETEffectContext effectContext =
                new UETEffectContext(
                    context.Attacker,
                    context.Victim,
                    context.Weapon,
                    qualityComp.Quality,
                    context.ArmorPiercingStrength);

            effectContext.Report(
                $"{context.Attacker.LabelShort}'s weapon tears through " +
                $"{context.Victim.LabelShort}'s defenses, ignoring " +
                $"{context.ArmorPiercingStrength:P0} of their armor.");

            UETCombatLog.Record(sundering, effectContext);
        }


    }
}

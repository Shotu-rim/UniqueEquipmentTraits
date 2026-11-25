using RimWorld;
using UnityEngine;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    internal class SunderingEffect : UETEffect
    {
        private const float MaxEffectiveMass = 15f;
        private const float MassBreakpoint = 2f;
        private const float BreakpointStrength = 0.50f;

        internal static float GetEffectStrength(
            Thing weapon,
            QualityCategory quality)
        {
            if (weapon == null)
                return 0f;

            float maxStrength;

            switch (quality)
            {
                case QualityCategory.Masterwork:
                    maxStrength = 0.40f;
                    break;

                case QualityCategory.Legendary:
                    maxStrength = 0.50f;
                    break;

                default:
                    maxStrength = WeaponTraitQuality.GetEffectStrength(quality);
                    break;
            }

            float mass = Mathf.Clamp(
                weapon.GetStatValue(StatDefOf.Mass),
                0f,
                MaxEffectiveMass);

            float massFactor;

            if (mass <= MassBreakpoint)
            {
                massFactor = Mathf.Lerp(
                    0f,
                    BreakpointStrength,
                    mass / MassBreakpoint);
            }
            else
            {
                massFactor = Mathf.Lerp(
                    BreakpointStrength,
                    1f,
                    (mass - MassBreakpoint) /
                    (MaxEffectiveMass - MassBreakpoint));
            }

            return maxStrength * massFactor;
        }

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

            float strength = GetEffectStrength(
                context.Weapon,
                qualityComp.Quality);

            if (strength <= 0f)
                return;

            context.ArmorPiercingActivated = true;
            context.ArmorPiercingStrength = strength;
        }

        public override void Apply(UETEffectContext context)
        {
            // Sundering triggers before damage
        }



        internal static void RecordCombatLog(MeleeAttackContext context)
        {
            if (context == null ||
                !context.ArmorPiercingActivated ||
                !context.ArmorPiercingApplied ||
                context.Attacker == null ||
                context.Victim == null ||
                context.Weapon == null)
                    return;

            RimWorld.WeaponTraitDef sundering = null;

            // Sundering selected by Prismatic
            if (context.PrismaticSelectedTrait != null &&
                context.PrismaticSelectedTrait.defName == "UET_Sundering" &&
                context.PrismaticActivated)
            {
                sundering = context.PrismaticSelectedTrait;
            }
            else
            {
                // Sundering present on weapon
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
                $"{context.ArmorPiercingStrength:P1} of their armor.");

            UETCombatLog.Record(sundering, effectContext);
        }


    }
}

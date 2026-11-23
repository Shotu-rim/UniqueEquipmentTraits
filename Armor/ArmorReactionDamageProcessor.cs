
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class ArmorReactionDamageProcessor
    {
        internal static void Process(
            ArmorReactionDamageObserver.DamageFrame frame)
        {
            if (frame == null ||
                frame.Defender == null ||
                frame.Defender.Dead ||
                frame.Defender.apparel == null ||
                frame.Injuries.Count == 0)
            {
                return;
            }

            Pawn defender = frame.Defender;

            List<Apparel> candidates = new List<Apparel>();

            foreach (Apparel apparel in defender.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null ||
                    !comp.CanActivateReactive() ||
                    comp.Traits.Count == 0)
                {
                    continue;
                }

                ArmorTraitDef trait = comp.Traits[0];

                if (trait == null)
                    continue;

                bool isHemocatalyst =
                    trait.defName == "UET_Hemocatalyst";

                bool isThermalVent =
                    trait.defName == "UET_ThermalVent";

                if (!isHemocatalyst && !isThermalVent)
                    continue;

                // Thermal Vent cannot trigger another Thermal Vent
                if (isThermalVent && UETThermalVentGuard.IsActive)
                    continue;

                float coveredDamage = GetCoveredDamage(
                    apparel,
                    frame.Injuries);

                if (coveredDamage <= 0f)
                    continue;

                candidates.Add(apparel);
            }

            if (candidates.Count == 0)
                return;

            candidates.Shuffle();

            int stackNumber = 0;

            foreach (Apparel apparel in candidates)
            {
                if (stackNumber >= 3)
                    break;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                ArmorTraitDef trait = comp.Traits[0];
                // Check the current state for previous reactions nested damage
                if (!comp.CanActivateReactive())
                    continue;

                if (trait.defName == "UET_ThermalVent" &&
                    UETThermalVentGuard.IsActive)
                {
                    continue;
                }

                float coveredDamage = GetCoveredDamage(
                    apparel,
                    frame.Injuries);

                BodyPartRecord hitPart =
                    GetFirstCoveredPart(
                        apparel,
                        frame.Injuries);

                stackNumber++;

                if (!ArmorReactionActivation.TryActivate(stackNumber))
                    continue;

                ArmorTraitActivationContext context =
                    new ArmorTraitActivationContext(
                        frame.DamageInfo.Instigator as Pawn,
                        defender,
                        apparel,
                        trait,
                        hitPart,
                        stackNumber,
                        frame.DamageInfo,
                        coveredDamage);

                ArmorTraitWorker worker = trait.Worker;

                if (!worker.CanActivate(context))
                    continue;

                float chance =
                    worker.GetActivationChance(context);

                if (chance <= 0f ||
                    !Rand.Chance(chance))
                {
                    continue;
                }

                if (!worker.TryActivate(context))
                    continue;

                if (!comp.State.TryConsumeCharge())
                    continue;

                comp.State.StartActivation(
                    trait.activeEffectTicks,
                    worker.GetCooldownTicks(context));

                comp.State.SetLastTriggeredPart(hitPart);
            }
        }

        private static float GetCoveredDamage(
            Apparel apparel,
            List<ArmorReactionDamageObserver.InjuryRecord> injuries)
        {
            float total = 0f;

            foreach (ArmorReactionDamageObserver.InjuryRecord injury
                in injuries)
            {
                if (ArmorTraitCoverage.CoversBodyPart(
                    apparel.def,
                    injury.Part))
                {
                    total += injury.Damage;
                }
            }

            return total;
        }

        private static BodyPartRecord GetFirstCoveredPart(
            Apparel apparel,
            List<ArmorReactionDamageObserver.InjuryRecord> injuries)
        {
            foreach (ArmorReactionDamageObserver.InjuryRecord injury
                in injuries)
            {
                if (ArmorTraitCoverage.CoversBodyPart(
                    apparel.def,
                    injury.Part))
                {
                    return injury.Part;
                }
            }

            return null;
        }
    }
}

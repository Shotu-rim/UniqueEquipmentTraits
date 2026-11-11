
using RimWorld;
using Verse;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;

namespace UniqueEquipmentTraits.Traits
{
    public class CompWeaponTrait : ThingComp
    {
        // Wyłącznie do ręcznego testowania traitów.
        private bool DebugSetTrait(RimWorld.WeaponTraitDef newTrait)
        {
            CompUniqueWeapon uniqueComp =
                parent.TryGetComp<CompUniqueWeapon>();

            if (uniqueComp == null ||
                uniqueComp.TraitsListForReading == null)
            {
                Messages.Message(
                    "[UET] Only unique items can have UET traits.",
                    MessageTypeDefOf.RejectInput,
                    false);
                return false;
            }

            List<RimWorld.WeaponTraitDef> traits =
                uniqueComp.TraitsListForReading;

            RimWorld.WeaponTraitDef oldTrait = null;

            foreach (RimWorld.WeaponTraitDef trait in traits)
            {
                if (trait != null &&
                    trait.GetModExtension<UETTraitExtension>() != null)
                {
                    oldTrait = trait;
                    break;
                }
            }

            if (newTrait == oldTrait)
                return true;

            if (newTrait != null)
            {
                if (newTrait.GetModExtension<UETTraitExtension>() == null)
                    return false;

                if (newTrait.abilityProps != null)
                {
                    Messages.Message(
                        "[UET] Trait uses unsupported abilityProps.",
                        MessageTypeDefOf.RejectInput,
                        false);
                    return false;
                }

                if (!uniqueComp.Props.weaponCategories.Contains(
                        newTrait.weaponCategory))
                    return false;

                foreach (RimWorld.WeaponTraitDef trait in traits)
                {
                    if (trait == oldTrait)
                        continue;

                    if (newTrait.Overlaps(trait))
                    {
                        Messages.Message(
                            "[UET] Trait conflicts with an existing trait.",
                            MessageTypeDefOf.RejectInput,
                            false);
                        return false;
                    }
                }

                if (oldTrait == null && traits.Count >= 3)
                {
                    Messages.Message(
                        "[UET] Maximum of three traits reached.",
                        MessageTypeDefOf.RejectInput,
                        false);
                    return false;
                }
            }

            if (oldTrait != null)
                traits.Remove(oldTrait);

            if (newTrait != null)
                uniqueComp.AddTrait(newTrait);

            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!Prefs.DevMode)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "UET: Set weapon trait",
                defaultDesc =
                    "Assign or replace a UET trait for testing.",
                action = OpenDebugTraitMenu
            };
        }

        private void OpenDebugTraitMenu()
        {
            List<FloatMenuOption> options =
                new List<FloatMenuOption>();

            foreach (RimWorld.WeaponTraitDef trait
                in DefDatabase<RimWorld.WeaponTraitDef>
                    .AllDefsListForReading)
            {
                if (trait.weaponCategory == null ||
                    trait.weaponCategory.defName != "UET_Melee")
                    continue;

                if (trait.GetModExtension<UETTraitExtension>() == null)
                    continue;

                RimWorld.WeaponTraitDef selectedTrait = trait;

                options.Add(new FloatMenuOption(
                    selectedTrait.label.CapitalizeFirst(),
                    () =>
                    {
                        if (DebugSetTrait(selectedTrait))
                        {
                            Messages.Message(
                                "[UET] Assigned trait: " +
                                selectedTrait.label,
                                MessageTypeDefOf.TaskCompletion,
                                false);
                        }
                    }));
            }

            options.Add(new FloatMenuOption(
                "Remove UET trait",
                () =>
                {
                    if (DebugSetTrait(null))
                    {
                        Messages.Message(
                            "[UET] Trait removed.",
                            MessageTypeDefOf.TaskCompletion,
                            false);
                    }
                }));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            RimWorld.WeaponTraitDef trait = null;

            // Statystyki wyłącznie dla natywnych Unique Weapons Odyssey.
            CompUniqueWeapon uniqueComp =
                parent.TryGetComp<CompUniqueWeapon>();

            if (uniqueComp == null ||
                uniqueComp.TraitsListForReading == null)
                yield break;

            foreach (RimWorld.WeaponTraitDef candidate
                in uniqueComp.TraitsListForReading)
            {
                if (candidate != null &&
                    candidate.GetModExtension<UETTraitExtension>() != null)
                {
                    trait = candidate;
                    break;
                }
            }

            if (trait == null)
                yield break;

            // Odyssey samodzielnie wyświetla nazwę traita.
            // UET pokazuje tylko dodatkowe parametry.
            CompQuality qualityComp =
                parent.TryGetComp<CompQuality>();

            if (qualityComp == null)
                yield break;

            float activationChance =
                WeaponTraitQuality.GetActivationChance(
                    qualityComp.Quality);

            yield return new StatDrawEntry(
                StatCategoryDefOf.BasicsImportant,
                "UET_ActivationChance".Translate(),
                activationChance.ToString("P0"),
                "UET_ActivationChanceDesc".Translate(),
                99996
            );

            UETTraitExtension extension =
                trait.GetModExtension<UETTraitExtension>();

            if (extension != null && extension.effectClass != null)
            {
                float effectStrength =
                    WeaponTraitQuality.GetEffectStrength(
                        qualityComp.Quality);

                yield return new StatDrawEntry(
                    StatCategoryDefOf.BasicsImportant,
                    "UET_EffectStrength".Translate(),
                    effectStrength.ToString("P0"),
                    "UET_EffectStrengthDesc".Translate(),
                    99995
                );
            }
        }
    }
}

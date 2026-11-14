
using RimWorld;
using Verse;
using System.Collections.Generic;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Effects;

namespace UniqueEquipmentTraits.Traits
{
    public class CompWeaponTrait : ThingComp
    {
        // Flurry is stored on weapon only
        private int flurryCharges;
        private Pawn flurryOwner;
        private bool flurryLastAttack;

        // Tailwind is stored on weapon only
        private int tailwindCharges;
        private Pawn tailwindOwner;

        public int TailwindCharges => tailwindCharges;

        public int FlurryCharges => flurryCharges;
        public bool FlurryLastAttack => flurryLastAttack;

        public override void CompTickRare()
        {
            base.CompTickRare();
            ValidateFlurryState();
            ValidateTailwindState();
        }

        private void ValidateFlurryState()
        {
            if (flurryCharges <= 0)
            {
                if (flurryOwner != null) ClearFlurry();
                return;
            }
            if (!FlurryActiveFor(flurryOwner))
                ClearFlurry();
            else
                SyncFlurryHediff(flurryOwner);
        }

        private void ValidateTailwindState()
        {
            if (tailwindCharges <= 0)
            {
                if (tailwindOwner != null)
                    ClearTailwind();

                return;
            }

            if (!TailwindActiveFor(tailwindOwner))
                ClearTailwind();
            else
                TailwindEffect.SyncHediff(tailwindOwner);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref flurryCharges, "uetFlurryCharges", 0);
            Scribe_References.Look(ref flurryOwner, "uetFlurryOwner");
            Scribe_Values.Look(ref flurryLastAttack, "uetFlurryLastAttack", false);
            Scribe_Values.Look(ref tailwindCharges, "uetTailwindCharges", 0);
            Scribe_References.Look(ref tailwindOwner, "uetTailwindOwner");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (flurryCharges < 0)
                    flurryCharges = 0;

                if (tailwindCharges < 0)
                    tailwindCharges = 0;
            }
        }

        public bool HasFlurry()
        {
            return FlurryEffect.HasFlurry(parent);
        }

        public void ClearFlurry()
        {
            FlurryEffect.RemoveHediff(flurryOwner);

            flurryCharges = 0;
            flurryOwner = null;
            flurryLastAttack = false;
        }

        // Activation after damage, triggering attack is not accelerated

        public bool TryActivateFlurry(Pawn pawn)
        {
            if (!FlurryEffect.IsValidOwner(parent, pawn))
                return false;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return false;

            float chance =
                FlurryEffect.ActivationChance(quality.Quality);

            if (!Rand.Chance(chance))
                return false;

            return ActivateFlurry(pawn);
        }

        // Prismatic call
        public bool ActivatePrismaticFlurry(Pawn pawn)
        {
            if (!PrismaticEffect.HasPrismatic(parent))
                return false;

            return ActivateFlurry(pawn);
        }

        private bool ActivateFlurry(Pawn pawn)
        {
            if (flurryCharges > 0 ||
                !FlurryEffect.CanUseEffect(parent, pawn))
                return false;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return false;

            int charges =
                FlurryEffect.InitialCharges(quality.Quality);

            if (charges <= 0)
                return false;

            flurryOwner = pawn;
            flurryCharges = charges;

            SyncFlurryHediff(pawn);

            return true;
        }



        public bool FlurryActiveFor(Pawn pawn)
        {
            return flurryCharges > 0 &&
                   flurryOwner == pawn &&
                   FlurryEffect.CanUseEffect(parent, pawn);
        }


        // Consume charge whether attack hits or fails
        public void ConsumeFlurryAttempt(Pawn pawn)
        {
            if (!FlurryActiveFor(pawn))
                return;

            flurryCharges--;

            

            if (flurryCharges == 0)
            {
                

                ClearFlurry();
            }
            else
            {
                SyncFlurryHediff(pawn);
            }
        }

        private void SyncFlurryHediff(Pawn pawn)
        {
            FlurryEffect.SyncHediff(pawn);
        }

        public float FlurrySpeedBonus()
        {
            CompQuality quality = parent.TryGetComp<CompQuality>();

            return quality == null
                ? 0f
                : FlurryEffect.SpeedBonus(quality.Quality);
        }

        public float FlurryDamageMultiplier()
        {
            CompQuality quality = parent.TryGetComp<CompQuality>();

            return quality == null
                ? 1f
                : FlurryEffect.DamageMultiplier(quality.Quality);
        }


        public bool HasTailwind()
        {
            return TailwindEffect.HasTailwind(parent);
        }

        // Removes effect
        public void ClearTailwind()
        {
            TailwindEffect.RemoveHediff(tailwindOwner);

            tailwindCharges = 0;
            tailwindOwner = null;
        }

        // Active after successful attack

        public bool TryActivateTailwind(Pawn pawn)
        {
            if (!TailwindEffect.IsValidOwner(parent, pawn))
                return false;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return false;

            float chance =
                TailwindEffect.ActivationChance(quality.Quality);

            if (!Rand.Chance(chance))
                return false;

            return ActivateTailwind(pawn);
        }

        // Prismatic call
        public bool ActivatePrismaticTailwind(Pawn pawn)
        {
            if (!PrismaticEffect.HasPrismatic(parent))
                return false;

            return ActivateTailwind(pawn);
        }

        private bool ActivateTailwind(Pawn pawn)
        {
            if (tailwindCharges > 0 ||
                !TailwindEffect.CanUseEffect(parent, pawn))
                return false;

            CompQuality quality = parent.TryGetComp<CompQuality>();

            if (quality == null)
                return false;

            int charges =
                TailwindEffect.InitialCharges(quality.Quality);

            if (charges <= 0)
                return false;

            tailwindOwner = pawn;
            tailwindCharges = charges;

            TailwindEffect.SyncHediff(pawn);

            return true;
        }


        // Check the owner for Tailwind
        public bool TailwindActiveFor(Pawn pawn)
        {
            return tailwindCharges > 0 &&
                   tailwindOwner == pawn &&
                   TailwindEffect.CanUseEffect(parent, pawn);
        }

        // Consumes charge after being attacked
        public void ConsumeTailwindAttempt(Pawn pawn)
        {
            if (!TailwindActiveFor(pawn))
                return;

            tailwindCharges--;

            if (tailwindCharges <= 0)
                ClearTailwind();
            else
                TailwindEffect.SyncHediff(pawn);
        }

        public float TailwindDodgeBonus()
        {
            CompQuality quality =
                parent.TryGetComp<CompQuality>();

            return quality == null
                ? 0f
                : TailwindEffect.DodgeBonus(quality.Quality);
        }

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

            ClearFlurry();
            ClearTailwind();

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

            // Show trait
            CompQuality qualityComp =
                parent.TryGetComp<CompQuality>();

            if (qualityComp == null)
                yield break;


            float activationChance =
                WeaponTraitQuality.GetActivationChance(
                    qualityComp.Quality);

            if (trait.defName == "UET_Flurry")
            {
                activationChance =
                    WeaponTraitQuality.GetEffectStrength(
                        qualityComp.Quality);
            }
            else if (trait.defName == "UET_Tailwind")
            {
                activationChance =
                    TailwindEffect.ActivationChance(
                        qualityComp.Quality);
            }
            else if (trait.defName == "UET_Prismatic")
            {
                activationChance =
                    UnityEngine.Mathf.Clamp01(
                        WeaponTraitQuality.GetEffectStrength(
                            qualityComp.Quality) * 2f);
            }
            else if (trait.defName == "UET_NaniteDisruptor")
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

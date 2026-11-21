
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Configures the armor trait component for apparel.
    /// Registers CompArmorTraits as the component implementation.
    /// Other mods can use this class when adding UET armor trait support.
    /// </summary>
    public class CompProperties_ArmorTraits : CompProperties
    {
        public CompProperties_ArmorTraits()
        {
            compClass = typeof(CompArmorTraits);
        }
    }

    /// <summary>
    /// Stores one armor trait and its persistent runtime state
    /// on an individual apparel item.
    /// Other mods can use AddTrait(), Traits and State
    /// to integrate custom armor traits.
    /// </summary>
    public class CompArmorTraits : ThingComp
    {
        private List<ArmorTraitDef> traits =
            new List<ArmorTraitDef>();

        private ArmorTraitState state =
            new ArmorTraitState();
    
        public IReadOnlyList<ArmorTraitDef> Traits
        {
            get { return traits; }
        }

        public ArmorTraitState State
        {
            get { return state; }
        }
        private float immunityDamageRemainder = 0f;
        public bool HasReactiveTrait
        {
            get
            {
                foreach (ArmorTraitDef trait in traits)
                {
                    if (trait != null &&
                        trait.activationType ==
                        ArmorTraitActivationType.Reactive)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        //public override void CompTickRare()
        //{
        //    base.CompTickRare();

        //    if (state == null ||
        //        traits == null ||
        //        traits.Count == 0)
        //    {
        //        return;
        //    }

        //    Apparel apparel = parent as Apparel;

        //    if (apparel == null)
        //        return;

        //    Pawn wearer = apparel.Wearer;

        //    if (wearer == null || wearer.health == null)
        //        return;

        //    foreach (ArmorTraitDef trait in traits)
        //    {
        //        if (trait == null ||
        //            trait.activationType !=
        //                ArmorTraitActivationType.Reactive)
        //        {
        //            continue;
        //        }

        //        if (state.ActiveEffectEndTick < 0)
        //            continue;

        //        if (state.IsEffectActive)
        //            continue;

        //        ArmorTraitHediffUtility.Remove(
        //            wearer,
        //            apparel,
        //            trait);
        //    }
        //}

        public override void CompTickRare()
        {
            base.CompTickRare();

            Apparel apparel = parent as Apparel;

            if (apparel == null || apparel.Wearer == null)
                return;

        }


        /// <summary>
        /// Checks whether this apparel item can participate
        /// in reactive armor trait selection.
        /// </summary>

        public bool CanActivateReactive()
        {
            if (!HasReactiveTrait || state == null)
                return false;

            ArmorTraitDef trait = null;

            foreach (ArmorTraitDef candidate in traits)
            {
                if (candidate != null &&
                    candidate.activationType ==
                    ArmorTraitActivationType.Reactive)
                {
                    trait = candidate;
                    break;
                }
            }

            if (trait == null)
                return false;

            if (state.IsEffectActive)
            {
                if (trait.blocksReactivationWhileActive)
                    return false;
            }
            else if (state.IsOnCooldown)
            {
                return false;
            }

            if (!state.HasAvailableCharge)
                return false;

            return true;
        }



        public bool AddTrait(ArmorTraitDef trait)
        {
            if (trait == null ||
                traits == null ||
                traits.Count > 0)
            {
                return false;
            }

            if (parent == null ||
                !ArmorTraitCompatibility.CanApply(
                    parent, trait))
            {
                return false;
            }

            traits.Add(trait);

            state = new ArmorTraitState();

            QualityCategory quality;

            if (parent.TryGetQuality(out quality))
            {
                int maximum =
                    ArmorTraitChargeUtility.GetMaxCharges(
                        trait,
                        quality);

                state.ConfigureCharges(maximum, quality);
            }

            return true;
        }
        public int AccumulateImmunityDamage(float damage)
        {
            if (damage <= 0f)
                return 0;

            immunityDamageRemainder += damage;

            int wholeDamage =
                Mathf.FloorToInt(immunityDamageRemainder + 0.000001f);

            immunityDamageRemainder -= wholeDamage;

            if (immunityDamageRemainder < 0f)
                immunityDamageRemainder = 0f;

            return wholeDamage;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(
                ref traits,
                "uetArmorTraits",
                LookMode.Def);

            Scribe_Deep.Look(
                ref state,
                "uetArmorTraitState");
            Scribe_Values.Look(
                ref immunityDamageRemainder,
                "uetImmunityDamageRemainder",
                0f);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (traits == null)
                    traits = new List<ArmorTraitDef>();

                if (state == null)
                    state = new ArmorTraitState();
            }
        }

        public override string CompInspectStringExtra()
        {
            if (traits == null || traits.Count == 0)
                return null;

            return "WeaponTraits".Translate() + ": " +
                traits.Where(t => t != null)
                      .Select(t => t.label)
                      .ToCommaList()
                      .CapitalizeFirst();
        }


        public override float GetStatFactor(StatDef stat)
        {
            float factor = base.GetStatFactor(stat);

            if (traits == null || traits.Count == 0)
                return factor;

            bool isArmor =
                stat == StatDefOf.ArmorRating_Sharp ||
                stat == StatDefOf.ArmorRating_Blunt ||
                stat == StatDefOf.ArmorRating_Heat;

            bool isMass = stat == StatDefOf.Mass;

            if (!isArmor && !isMass)
                return factor;

            QualityCategory quality;

            if (parent == null ||
                !parent.TryGetQuality(out quality))
                return factor;

            foreach (ArmorTraitDef trait in traits)
            {
                if (trait == null)
                    continue;

                float bonus = 0f;
                float reduction = 0f;

                if (quality == QualityCategory.Masterwork)
                {
                    bonus = isArmor
                        ? trait.masterworkArmorRatingBonus
                        : trait.masterworkMassIncrease;

                    reduction = isArmor
                        ? trait.masterworkLightweightArmorPenalty
                        : trait.masterworkLightweightMassReduction;
                }
                else if (quality == QualityCategory.Legendary)
                {
                    bonus = isArmor
                        ? trait.legendaryArmorRatingBonus
                        : trait.legendaryMassIncrease;

                    reduction = isArmor
                        ? trait.legendaryLightweightArmorPenalty
                        : trait.legendaryLightweightMassReduction;
                }

                // Reinforced: increases armor rating or mass
                if (bonus > 0f)
                    factor *= 1f + bonus;

                // Lightweight: decreases armor rating or mass
                if (reduction > 0f)
                    factor *= 1f - UnityEngine.Mathf.Clamp01(reduction);
            }

            return factor;
        }
        // Armor buttons
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            if (traits == null || traits.Count == 0)
                yield break;

            QualityCategory quality;

            if (!parent.TryGetQuality(out quality))
                yield break;

            if (quality != QualityCategory.Legendary)
                yield break;

            foreach (ArmorTraitDef trait in traits)
            {
                if (trait == null ||
                    trait.defName != "UET_NeuralDeafness")
                    continue;

                if (state == null)
                    continue;

                int currentCharges = state.Charges;
                int maximumCharges = state.MaxCharges;

                Command_Action command = new Command_Action
                {
                    defaultLabel = "Silence",

                    icon = ContentFinder<Texture2D>.Get("UI/Commands/UET_Silence"),

                    defaultDesc =
                        "Temporarily suppresses the target's psychic sensitivity.\n\n" +
                        "Charges: " + currentCharges + "/" + maximumCharges,

                    action = delegate
                    {
                        Pawn caster = (parent as Apparel)?.Wearer;

                        if (caster == null || caster.Map == null || caster.Dead)
                            return;

                        TargetingParameters targetParams = new TargetingParameters
                        {
                            canTargetPawns = true,
                            canTargetBuildings = false,
                            canTargetItems = false,
                            canTargetLocations = false
                        };

                        Find.Targeter.BeginTargeting(
                            targetParams,
                            delegate (LocalTargetInfo target)
                            {
                                Pawn targetPawn = target.Pawn;

                                if (targetPawn == null ||
                                    targetPawn.Dead ||
                                    targetPawn.Map != caster.Map)
                                    return;

                                if (!caster.Position.InHorDistOf(
                                    targetPawn.Position, 32f))
                                    return;

                                // Check if the caster still wears this apparel
                                if ((parent as Apparel)?.Wearer != caster)
                                    return;

                                if (state.Charges <= 0 ||
                                    !state.TryConsumeCharge())
                                    return;

                                HediffDef silenceDef =
                                    DefDatabase<HediffDef>.GetNamed("UET_Silence");

                                Hediff existingSilence =
                                    targetPawn.health.hediffSet.GetFirstHediffOfDef(silenceDef);

                                if (existingSilence != null)
                                {
                                    HediffComp_Disappears disappears =
                                        existingSilence.TryGetComp<HediffComp_Disappears>();

                                    if (disappears != null)
                                        disappears.ticksToDisappear = 7200;
                                }
                                else
                                {
                                    Hediff silence =
                                        HediffMaker.MakeHediff(silenceDef, targetPawn);

                                    targetPawn.health.AddHediff(silence);
                                }

                                Messages.Message(
                                    "UET: Silence applied to " +
                                    targetPawn.LabelShort,
                                    MessageTypeDefOf.NeutralEvent,
                                    false);
                            },
                            null,
                            delegate (LocalTargetInfo target)
                            {
                                Pawn targetPawn = target.Pawn;

                                return targetPawn != null &&
                                       !targetPawn.Dead &&
                                       targetPawn.Map == caster.Map &&
                                       caster.Position.InHorDistOf(
                                           targetPawn.Position, 32f);
                            },
                            caster
                        );
                    }
                };

                if (maximumCharges <= 0)
                {
                    command.Disable("No charges configured.");
                }
                else if (currentCharges <= 0)
                {
                    command.Disable("No charges available.");
                }

                yield return command;
            }
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (traits == null || traits.Count == 0)
                yield break;

            List<ArmorTraitDef> validTraits =
                traits.Where(t => t != null).ToList();

            if (validTraits.Count == 0)
                yield break;

            StringBuilder description =
                new StringBuilder();

            description.AppendLine(
                "Stat_ThingUniqueWeaponTrait_Desc".Translate());

            description.AppendLine();


            foreach (ArmorTraitDef trait in validTraits)
            {
                description.AppendLine(
                    trait.LabelCap.Colorize(
                        ColorLibrary.Yellow));

                description.AppendLine(
                    trait.description);

                if (trait.boostedSkill != null)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        int bonus = 0;

                        if (quality == QualityCategory.Masterwork &&
                            !trait.legendaryOnly)
                        {
                            bonus = trait.masterworkSkillBonus;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            bonus = trait.legendarySkillBonus;
                        }

                        if (bonus > 0)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                (trait.boostedSkill.LabelCap +
                                ": +" + bonus)
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }


                if (trait.masterworkDodgeMultiplier > 0f ||
                    trait.legendaryDodgeMultiplier > 0f ||
                    trait.masterworkDodgeFlatBonus > 0f ||
                    trait.legendaryDodgeFlatBonus > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float multiplier = 0f;
                        float flatBonus = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            multiplier =
                                trait.masterworkDodgeMultiplier;

                            flatBonus =
                                trait.masterworkDodgeFlatBonus;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            multiplier =
                                trait.legendaryDodgeMultiplier;

                            flatBonus =
                                trait.legendaryDodgeFlatBonus;
                        }

                        if (multiplier > 0f || flatBonus > 0f)
                        {
                            description.AppendLine();

                            if (multiplier > 0f)
                            {
                                description.AppendLine(
                                    ("Melee dodge chance: +" +
                                    (multiplier * 100f).ToString("0.#") +
                                    "% (relative)")
                                    .Colorize(ColorLibrary.Green));
                            }

                            if (flatBonus > 0f)
                            {
                                description.AppendLine(
                                    ("Melee dodge chance: +" +
                                    (flatBonus * 100f).ToString("0.#") +
                                    " percentage points")
                                    .Colorize(ColorLibrary.Green));
                            }
                        }
                    }
                }

                if (trait.masterworkCriticalChance > 0f ||
                    trait.legendaryCriticalChance > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float criticalChance = 0f;
                        float criticalDamageBonus = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            criticalChance =
                                trait.masterworkCriticalChance;

                            criticalDamageBonus =
                                trait.masterworkCriticalDamageBonus;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            criticalChance =
                                trait.legendaryCriticalChance;

                            criticalDamageBonus =
                                trait.legendaryCriticalDamageBonus;
                        }

                        if (criticalChance > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Critical hit chance: +" +
                                (criticalChance * 100f).ToString("0.##") +
                                " percentage points")
                                .Colorize(ColorLibrary.Green));

                            description.AppendLine(
                                ("Critical damage bonus: +" +
                                (criticalDamageBonus * 100f).ToString("0.##") +
                                "%")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }

                if (trait.masterworkNeuralHeatRecoveryBonus > 0f ||
                    trait.legendaryNeuralHeatRecoveryBonus > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float recoveryBonus = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            recoveryBonus =
                                trait.masterworkNeuralHeatRecoveryBonus;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            recoveryBonus =
                                trait.legendaryNeuralHeatRecoveryBonus;
                        }

                        if (recoveryBonus > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Neural heat recovery rate: +" +
                                (recoveryBonus * 100f).ToString("0.##") +
                                "%")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }

                if (trait.masterworkImmunityGainBonus > 0f ||
                    trait.legendaryImmunityGainBonus > 0f ||
                    trait.masterworkDiseaseRiskReduction > 0f ||
                    trait.legendaryDiseaseRiskReduction > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float immunityBonus = 0f;
                        float diseaseReduction = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            immunityBonus =
                                trait.masterworkImmunityGainBonus;

                            diseaseReduction =
                                trait.masterworkDiseaseRiskReduction;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            immunityBonus =
                                trait.legendaryImmunityGainBonus;

                            diseaseReduction =
                                trait.legendaryDiseaseRiskReduction;
                        }

                        if (immunityBonus > 0f ||
                            diseaseReduction > 0f)
                        {
                            description.AppendLine();

                            if (immunityBonus > 0f)
                            {
                                description.AppendLine(
                                    ("Immunity gain speed: +" +
                                    (immunityBonus * 100f).ToString("0.##") +
                                    "%")
                                    .Colorize(ColorLibrary.Green));
                            }

                            if (diseaseReduction > 0f)
                            {
                                description.AppendLine(
                                    ("Disease contraction chance: -" +
                                    (diseaseReduction * 100f).ToString("0.##") +
                                    "% (relative)")
                                    .Colorize(ColorLibrary.Green));
                            }
                        }
                    }
                }

                // Reinforced
                if (trait.masterworkArmorRatingBonus > 0f ||
                    trait.legendaryArmorRatingBonus > 0f ||
                    trait.masterworkMassIncrease > 0f ||
                    trait.legendaryMassIncrease > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float armorBonus = 0f;
                        float massIncrease = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            armorBonus =
                                trait.masterworkArmorRatingBonus;

                            massIncrease =
                                trait.masterworkMassIncrease;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            armorBonus =
                                trait.legendaryArmorRatingBonus;

                            massIncrease =
                                trait.legendaryMassIncrease;
                        }

                        if (armorBonus > 0f || massIncrease > 0f)
                        {
                            description.AppendLine();

                            if (armorBonus > 0f)
                            {
                                description.AppendLine(
                                    ("Armor Rating (Sharp, Blunt, Heat): +" +
                                    (armorBonus * 100f).ToString("0.##") +
                                    "%")
                                    .Colorize(ColorLibrary.Green));
                            }

                            if (massIncrease > 0f)
                            {
                                description.AppendLine(
                                    ("Mass: +" +
                                    (massIncrease * 100f).ToString("0.##") +
                                    "%")
                                    .Colorize(ColorLibrary.RedReadable));
                            }
                        }
                    }
                }

                // Lightweight
                if (trait.masterworkLightweightMassReduction > 0f ||
                    trait.legendaryLightweightMassReduction > 0f ||
                    trait.masterworkLightweightArmorPenalty > 0f ||
                    trait.legendaryLightweightArmorPenalty > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float massReduction = 0f;
                        float armorPenalty = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            massReduction =
                                trait.masterworkLightweightMassReduction;

                            armorPenalty =
                                trait.masterworkLightweightArmorPenalty;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            massReduction =
                                trait.legendaryLightweightMassReduction;

                            armorPenalty =
                                trait.legendaryLightweightArmorPenalty;
                        }

                        if (massReduction > 0f || armorPenalty > 0f)
                        {
                            description.AppendLine();

                            if (massReduction > 0f)
                            {
                                description.AppendLine(
                                    ("Mass: -" +
                                    (massReduction * 100f).ToString("0.##") +
                                    "%")
                                    .Colorize(ColorLibrary.Green));
                            }

                            if (armorPenalty > 0f)
                            {
                                description.AppendLine(
                                    ("Armor Rating (Sharp, Blunt, Heat): -" +
                                    (armorPenalty * 100f).ToString("0.##") +
                                    "%")
                                    .Colorize(ColorLibrary.RedReadable));
                            }
                        }
                    }
                }

                // Acceleration
                if (trait.meleeCooldownReduction > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float reduction = 0f;

                        if (quality == QualityCategory.Masterwork)
                            reduction = 0.15f;
                        else if (quality == QualityCategory.Legendary)
                            reduction = 0.25f;

                        if (reduction > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Melee cooldown: -" +
                                (reduction * 100f).ToString("0.##") +
                                "%")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }


                // Regeneration
                if (trait.masterworkRegenerationInterval > 0f ||
                    trait.legendaryRegenerationInterval > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float interval = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            interval =
                                trait.masterworkRegenerationInterval;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            interval =
                                trait.legendaryRegenerationInterval;
                        }

                        if (interval > 0f)
                        {
                            float healingPerMinute = 60f / interval;

                            description.AppendLine();

                            description.AppendLine(
                                ("Regeneration: +" +
                                healingPerMinute.ToString("0.##") +
                                " HP/min")
                                .Colorize(ColorLibrary.Green));

                            description.AppendLine(
                                ("Healing interval: " +
                                interval.ToString("0.##") +
                                " seconds")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }


                // Craftsmanship
                if (trait.masterworkCraftsmanshipChance > 0f ||
                    trait.legendaryCraftsmanshipChance > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float chance = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            chance =
                                trait.masterworkCraftsmanshipChance;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            chance =
                                trait.legendaryCraftsmanshipChance;
                        }

                        if (chance > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Crafted item quality: +" +
                                "1 level (" +
                                (chance * 100f).ToString("0.##") +
                                "% chance)")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }

                // Empowered
                if (trait.masterworkCapacityBonus > 0f ||
                    trait.legendaryCapacityBonus > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float bonus = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            bonus = trait.masterworkCapacityBonus;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            bonus = trait.legendaryCapacityBonus;
                        }

                        if (bonus > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Pawn capacities: +" +
                                (bonus * 100f).ToString("0.##") +
                                "% (relative)")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }

                // Neutrality
                if (trait.masterworkMentalBreakBlockChance > 0f ||
                    trait.legendaryMentalBreakBlockChance > 0f)
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float chance = 0f;

                        if (quality == QualityCategory.Masterwork)
                        {
                            chance =
                                trait.masterworkMentalBreakBlockChance;
                        }
                        else if (quality == QualityCategory.Legendary)
                        {
                            chance =
                                trait.legendaryMentalBreakBlockChance;
                        }

                        if (chance > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                ("Mental break prevention: " +
                                (chance * 100f).ToString("0.##") +
                                "% chance")
                                .Colorize(ColorLibrary.Green));
                        }
                    }
                }

                // Neural Deafness
                if (trait.defName == "UET_NeuralDeafness")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        if (quality == QualityCategory.Masterwork ||
                            quality == QualityCategory.Legendary)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                "Psychic sensitivity: 0%"
                                .Colorize(ColorLibrary.Green));
                        }

                        if (quality == QualityCategory.Legendary)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                "Ability: Silence"
                                .Colorize(ColorLibrary.Green));

                            description.AppendLine(
                                "Target psychic sensitivity: 0%");

                            description.AppendLine(
                                "Duration: 120 seconds");

                            description.AppendLine(
                                "Maximum charges: 2");

                        }
                    }
                }

                // Elemental and environmental immunities
                string immunityName = null;

                switch (trait.defName)
                {
                    case "UET_HeatImmunity":
                        immunityName = "Heat";
                        break;

                    case "UET_ColdImmunity":
                        immunityName = "Cold";
                        break;

                    case "UET_AcidImmunity":
                        immunityName = "Acid";
                        break;

                    case "UET_ElectricityImmunity":
                        immunityName = "Electricity";
                        break;

                    case "UET_ToxicImmunity":
                        immunityName = "Toxic";
                        break;

                    case "UET_VacuumImmunity":
                        immunityName = "Vacuum";
                        break;
                }
                // Resistances
                if (immunityName != null)
                {
                    QualityCategory immunityQuality;

                    if (parent != null &&
                        parent.TryGetQuality(out immunityQuality))
                    {
                        float reduction = 0f;

                        if (immunityQuality ==
                            QualityCategory.Masterwork)
                        {
                            reduction =
                                trait.masterworkImmunityReduction;
                        }
                        else if (immunityQuality ==
                                 QualityCategory.Legendary)
                        {
                            reduction =
                                trait.legendaryImmunityReduction;
                        }

                        reduction = Mathf.Clamp01(reduction);

                        if (reduction > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                (immunityName + " immunity: " +
                                 (reduction * 100f).ToString("0.##") +
                                 "% reduction")
                                .Colorize(ColorLibrary.Green));

                            description.AppendLine(
                                "Protects the wearer and this apparel item " +
                                "against applicable effects.");

                            description.AppendLine(
                                "Wearer protection uses the highest " +
                                "matching immunity; effects do not stack.");
                        }
                    }
                }

                float moveSpeedBonus =
                    ArmorTraitMoveSpeedUtility.GetItemBonus(
                        parent as Apparel,
                        trait);

                if (moveSpeedBonus > 0f)
                {
                    description.AppendLine();

                    description.AppendLine(
                        ("Movement speed: +" +
                        (moveSpeedBonus * 100f).ToString("0.##") +
                        "%")
                        .Colorize(ColorLibrary.Green));
                }


                description.AppendLine();
            }


            yield return new StatDrawEntry(
                StatCategoryDefOf.BasicsImportant,
                "Stat_ThingUniqueWeaponTrait_Label".Translate(),
                validTraits.Select(t => t.label)
                           .ToCommaList()
                           .CapitalizeFirst(),
                description.ToString(),
                1104);
        }

        public override string CompTipStringExtra()
        {
            if (traits == null || traits.Count == 0)
                return null;

            return "WeaponTraits".Translate() + ": " +
                traits.Where(t => t != null)
                      .Select(t => t.label)
                      .ToCommaList()
                      .CapitalizeFirst();
        }
    }
}

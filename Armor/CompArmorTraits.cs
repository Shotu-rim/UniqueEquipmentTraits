
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

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

                // Reinforced: increases armor rating or mass.
                if (bonus > 0f)
                    factor *= 1f + bonus;

                // Lightweight: decreases armor rating or mass.
                if (reduction > 0f)
                    factor *= 1f - UnityEngine.Mathf.Clamp01(reduction);
            }

            return factor;
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

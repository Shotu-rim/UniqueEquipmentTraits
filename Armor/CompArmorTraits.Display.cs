
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public partial class CompArmorTraits
    {
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


                // Overdrive
                if (trait.defName == "UET_Overdrive")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float speedBonus =
                            ArmorTraitOverdriveUtility.GetSpeedBonus(quality);

                        int durationTicks =
                            ArmorTraitOverdriveUtility.GetDurationTicks(quality);

                        int exhaustedTicks =
                            ArmorTraitOverdriveUtility.GetExhaustedTicks(quality);

                        float penalty =
                            ArmorTraitOverdriveUtility.GetExhaustedPenalty(quality);

                        int cooldownTicks =
                            ArmorTraitOverdriveUtility.GetCooldownTicks();

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(quality);

                        description.AppendLine();

                        description.AppendLine(
                            "Ability: Overdrive"
                            .Colorize(ColorLibrary.Green));

                    }
                }

                // Technomancer
                if (trait.defName == "UET_Technomancer")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float capacityBonus =
                            ArmorTraitTechnomancerUtility.GetCapacityBonus(
                                quality);

                        float radius =
                            ArmorTraitTechnomancerUtility.GetRadius(
                                quality);

                        int durationTicks =
                            ArmorTraitTechnomancerUtility.GetDurationTicks(
                                quality);

                        int cooldownTicks =
                            ArmorTraitTechnomancerUtility.GetCooldownTicks();

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        description.AppendLine();

                        description.AppendLine(
                            "Ability: Technomancer"
                            .Colorize(ColorLibrary.Green));

                    }
                }

                // Systemshock
                if (trait.defName == "UET_Systemshock")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float range =
                            ArmorTraitSystemshockUtility.GetRange(quality);

                        float radius =
                            ArmorTraitSystemshockUtility.GetRadius(quality);

                        int cooldownTicks =
                            ArmorTraitSystemshockUtility.GetCooldownTicks(quality);

                        int goodwillPenalty =
                            ArmorTraitSystemshockUtility.GetGoodwillPenalty(quality);

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(quality);

                        description.AppendLine();

                        description.AppendLine(
                            "Ability: Systemshock"
                            .Colorize(ColorLibrary.Green));

                    }
                }

                // Medic
                if (trait.defName == "UET_Medic")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float range =
                            ArmorTraitMedicUtility.GetRange(quality);

                        float healingPool =
                            ArmorTraitMedicUtility.GetHealingPool(quality);

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        description.AppendLine();
                        description.AppendLine(
                            "Ability: Medic".Colorize(ColorLibrary.Green));

                    }
                }

                // Berserk
                if (trait.defName == "UET_Berserk")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        if (quality == QualityCategory.Masterwork ||
                            quality == QualityCategory.Legendary)
                        {
                            int durationTicks =
                                ArmorTraitBerserkUtility.GetDurationTicks(
                                    quality,
                                    1f);

                            int maxDurationTicks =
                                ArmorTraitBerserkUtility.GetDurationTicks(
                                    quality,
                                    2f);

                            int goodwillPenalty =
                                ArmorTraitBerserkUtility.GetGoodwillPenalty(
                                    quality);

                            int maxCharges =
                                ArmorTraitChargeUtility.GetMaxCharges(
                                    trait,
                                    quality);

                            int regenerationTicks =
                                ArmorTraitState.GetChargeRegenTicks(
                                    quality);

                            description.AppendLine();

                            description.AppendLine(
                                "Ability: Berserk"
                                .Colorize(ColorLibrary.Green));

                            description.AppendLine(
                                "Base duration: " +
                                (durationTicks / 60f).ToString("0.#") +
                                " seconds (100% psychic sensitivity)");
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


                // Mind Cleansing
                if (trait.defName == "UET_MindCleansing")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        if (quality == QualityCategory.Masterwork ||
                            quality == QualityCategory.Legendary)
                        {
                            int paralysisTicks =
                                ArmorTraitMindCleansingUtility.GetParalysisTicks(
                                    quality);

                            int cooldownTicks =
                                ArmorTraitMindCleansingUtility.GetCooldownTicks(
                                    quality);

                            description.AppendLine();

                            description.AppendLine(
                                "Ability: Mind Cleansing"
                                .Colorize(ColorLibrary.Green));

                        }
                    }
                }

                // Kindle
                if (trait.defName == "UET_Kindle")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float range =
                            ArmorTraitKindleUtility.GetRange(quality);

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        int cooldownTicks =
                            ArmorTraitKindleUtility.GetCooldownTicks();

                        int goodwillPenalty =
                            ArmorTraitKindleUtility.GetGoodwillPenalty(
                                quality);

                        description.AppendLine();
                        description.AppendLine(
                            "Ability: Kindle".Colorize(ColorLibrary.Green));

                    }
                }
                // Toxic Cloud
                if (trait.defName == "UET_ToxicCloud")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float range =
                            ArmorTraitToxicCloudUtility.GetRange(quality);

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        int cooldownTicks =
                            ArmorTraitToxicCloudUtility.GetCooldownTicks();

                        int goodwillPenalty =
                            ArmorTraitToxicCloudUtility.GetGoodwillPenalty(
                                quality);

                        description.AppendLine();
                        description.AppendLine(
                            "Ability: Toxic Cloud".Colorize(ColorLibrary.Green));
                    }
                }

                // Stunning Pulse
                if (trait.defName == "UET_StunningPulse")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        int stunTicks =
                            ArmorTraitStunningPulseUtility.GetStunTicks(
                                quality);

                        float range =
                            ArmorTraitStunningPulseUtility.GetRange();

                        int cooldownTicks =
                            ArmorTraitStunningPulseUtility.GetCooldownTicks();

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        description.AppendLine();

                        description.AppendLine(
                            "Ability: Stunning Pulse"
                            .Colorize(ColorLibrary.Green));

                    }
                }

                // Shredding
                if (trait.defName == "UET_Shredding")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality) &&
                        (quality == QualityCategory.Masterwork ||
                         quality == QualityCategory.Legendary))
                    {
                        float bleedingMultiplier =
                            ArmorTraitShreddingUtility.GetBleedingMultiplier(
                                quality);

                        float range =
                            ArmorTraitMedicUtility.GetRange(quality);

                        int cooldownTicks =
                            ArmorTraitShreddingUtility.GetCooldownTicks();

                        int maxCharges =
                            ArmorTraitChargeUtility.GetMaxCharges(
                                trait,
                                quality);

                        int regenTicks =
                            ArmorTraitState.GetChargeRegenTicks(
                                quality);

                        description.AppendLine();

                        description.AppendLine(
                            "Ability: Shredding"
                            .Colorize(ColorLibrary.Green));

                    }
                }

                // Skybound
                if (trait.defName == "UET_Skybound")
                {
                    QualityCategory quality;

                    if (parent.TryGetQuality(out quality))
                    {
                        float range =
                            ArmorTraitSkyboundUtility.GetJumpRange(quality);

                        int cooldown =
                            ArmorTraitSkyboundUtility.GetCooldownTicks(quality);

                        if (range > 0f)
                        {
                            description.AppendLine();

                            description.AppendLine(
                                "Ability: Jump"
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

    }
}

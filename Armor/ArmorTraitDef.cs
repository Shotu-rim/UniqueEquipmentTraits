
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Defines how an armor trait operates: passively, in response
    /// to combat events, or through an activated ability.
    /// Other mods can use these values when defining custom armor traits.
    /// </summary>
    public enum ArmorTraitActivationType
    {
        Passive,
        Reactive,
        Activated
    }
    /// <summary>
    /// Defines an armor trait, including its activation type, anatomical
    /// restrictions, naming options, and effect worker.
    /// Other mods can create custom ArmorTraitDef entries and provide
    /// their own ArmorTraitWorker implementations.
    /// </summary>
    public class ArmorTraitDef : Def
    {
        public ArmorTraitActivationType activationType =
            ArmorTraitActivationType.Passive;
        public int cooldownTicks = 0;
        public int activeEffectTicks = 0;
        public bool blocksReactivationWhileActive = true;
        // Skill bonus configuration
        public SkillDef boostedSkill;
        public int masterworkSkillBonus = 1;
        public int legendarySkillBonus = 2;
        public bool legendaryOnly = false;
        public List<ArmorTraitCategory> allowedCategories;
        public List<StuffCategoryDef> allowedStuffCategories;
        public List<string> nameAdjectives;
        public List<string> nameConcepts;
        public float masterworkMentalBreakBlockChance = 0f;
        public float legendaryMentalBreakBlockChance = 0f;

        // Passive movement speed bonus configuration
        public float masterworkMoveSpeedBonus = 0f;
        public float legendaryMoveSpeedBonus = 0f;
        // Passive critical hit configuration
        public float masterworkCriticalChance = 0f;
        public float legendaryCriticalChance = 0f;
        public float masterworkCriticalDamageBonus = 0f;
        public float legendaryCriticalDamageBonus = 0f;
        // Passive neural heat recovery configuration
        public float masterworkNeuralHeatRecoveryBonus = 0f;
        public float legendaryNeuralHeatRecoveryBonus = 0f;
        // Passive illness handlers
        public float masterworkImmunityGainBonus = 0f;
        public float legendaryImmunityGainBonus = 0f;

        public float masterworkDiseaseRiskReduction = 0f;
        public float legendaryDiseaseRiskReduction = 0f;

        // Acceleration
        public float meleeCooldownReduction = 0f;

        // Productivity
        public float masterworkWorkSpeedBonus = 0f;
        public float legendaryWorkSpeedBonus = 0f;
        // Empowered
        public float masterworkCapacityBonus = 0f;
        public float legendaryCapacityBonus = 0f;
        // Craftsmanship
        public float masterworkCraftsmanshipChance = 0f;
        public float legendaryCraftsmanshipChance = 0f;


        // Regeneration
        public float masterworkRegenerationInterval = 0f;
        public float legendaryRegenerationInterval = 0f;

        // Lightweight
        public float masterworkLightweightMassReduction = 0f;
        public float legendaryLightweightMassReduction = 0f;
        public float masterworkLightweightArmorPenalty = 0f;
        public float legendaryLightweightArmorPenalty = 0f;
        // Global resistances - Fire/Cold/Elec/Tox/Acid/Vacuum
        public float masterworkImmunityReduction = 0.85f;
        public float legendaryImmunityReduction = 1f;

        // Reinforced
        public float masterworkArmorRatingBonus = 0f;
        public float legendaryArmorRatingBonus = 0f;

        public float masterworkMassIncrease = 0f;
        public float legendaryMassIncrease = 0f;


        // Passive dodge bonus configuration
        public float masterworkDodgeMultiplier = 0f;
        public float legendaryDodgeMultiplier = 0f;
        public float masterworkDodgeFlatBonus = 0f;
        public float legendaryDodgeFlatBonus = 0f;
        public float commonality = 1f;
        public bool canGenerateAlone = true;
        public List<string> exclusionTags;
        public bool usesCharges = false;

        public System.Type workerClass;

        private ArmorTraitWorker worker;

        public ArmorTraitWorker Worker
        {
            get
            {
                if (worker == null)
                {
                    System.Type type = workerClass ??
                        typeof(ArmorTraitWorker_Default);

                    worker = (ArmorTraitWorker)
                        System.Activator.CreateInstance(type);
                }

                return worker;
            }
        }
    }
}

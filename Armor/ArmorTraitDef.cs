
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

        public List<ArmorTraitCategory> allowedCategories;
        public List<StuffCategoryDef> allowedStuffCategories;
        public List<string> nameAdjectives;
        public List<string> nameConcepts;
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

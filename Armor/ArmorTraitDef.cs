
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

        public List<ArmorTraitCategory> allowedCategories;
        public List<string> nameAdjectives;
        public List<string> nameConcepts;

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

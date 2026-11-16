
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public enum ArmorTraitActivationType
    {
        Passive,
        Reactive,
        Activated
    }

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

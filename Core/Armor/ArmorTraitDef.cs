
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitDef : Def
    {
        // Czy trait aktywuje się w reakcji na trafienie?
        public bool reactive = false;

        public List<ArmorTraitCategory> allowedCategories;


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


using System.Collections.Generic;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class ArmorDamageInterceptionScope
    {
        [System.ThreadStatic]
        private static Stack<ArmorDamageInterceptionContext> contexts;

        public static ArmorDamageInterceptionContext Current
        {
            get
            {
                if (contexts == null || contexts.Count == 0)
                    return null;

                return contexts.Peek();
            }
        }

        public static void Begin()
        {
            if (contexts == null)
            {
                contexts =
                    new Stack<ArmorDamageInterceptionContext>();
            }

            contexts.Push(
                new ArmorDamageInterceptionContext());
        }

        public static void End()
        {
            if (contexts == null || contexts.Count == 0)
                return;

            contexts.Pop();
        }
    }
}

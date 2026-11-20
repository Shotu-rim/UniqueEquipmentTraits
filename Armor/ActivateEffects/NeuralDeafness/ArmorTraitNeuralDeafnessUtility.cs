using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitNeuralDeafnessUtility
    {
        public static bool HasNeuralDeafness(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null)
                return false;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null)
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait != null &&
                        trait.defName == "UET_NeuralDeafness")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
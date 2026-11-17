
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    internal static class ArmorReactionActivation
    {
        internal static float GetActivationChance(int stackNumber)
        {
            switch (stackNumber)
            {
                case 1:
                    return 1f;

                case 2:
                    return 0.40f;

                case 3:
                    return 0.25f;

                default:
                    return 0f;
            }
        }


        internal static bool TryActivate(int stackNumber)
        {
            float chance = GetActivationChance(stackNumber);

            if (chance <= 0f)
                return false;

            if (chance >= 1f)
                return true;

            return Rand.Chance(chance);
        }

    }
}

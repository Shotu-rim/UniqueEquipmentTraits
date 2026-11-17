
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Controls activation chances for stacked reactive armor traits.
    /// The first reaction always activates, while the second and third
    /// have a 40% and 25% activation chance respectively.
    /// Other mods can use GetActivationChance() or TryActivate()
    /// to apply the same probability rules to armor reactions.
    /// </summary>
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

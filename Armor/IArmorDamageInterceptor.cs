
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public interface IArmorDamageInterceptor
    {
        bool CanInterceptDamage(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait,
            float damage);

        bool TryInterceptDamage(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait,
            float damage);
    }
}

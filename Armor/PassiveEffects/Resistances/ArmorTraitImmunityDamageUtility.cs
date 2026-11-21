using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitImmunityDamageUtility
    {
        public static string GetImmunityTrait(DamageDef damageDef)
        {
            if (damageDef == null)
                return null;

            switch (damageDef.defName)
            {
                case "Burn":
                case "Flame":
                    return "UET_HeatImmunity";

                case "Frostbite":
                    return "UET_ColdImmunity";

                case "AcidBurn":
                case "VEF_SecondaryAcidBurn":
                    return "UET_AcidImmunity";

                case "ElectricalBurn":
                case "EMP":
                    return "UET_ElectricityImmunity";

                default:
                    return null;
            }
        }
    }
}

using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class UETDlcUtility
    {
        public static bool IsTraitAvailable(ArmorTraitDef trait)
        {
            return trait != null &&
                IsTraitAvailable(trait.defName);
        }

        public static bool IsTraitAvailable(WeaponTraitDef trait)
        {
            return trait != null &&
                IsTraitAvailable(trait.defName);
        }

        public static bool IsTraitAvailable(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return false;

            switch (defName)
            {
                // Royalty
                case "UET_Disruptive":
                case "UET_Mindflow":
                case "UET_MindCleansing":
                case "UET_Mindweave":
                case "UET_Hemocatalyst":
                case "UET_NeuralDeafness":
                    return ModsConfig.RoyaltyActive;

                // Biotech
                case "UET_Technomancer":
                case "UET_ToxicCloud":
                    return ModsConfig.BiotechActive;

                // Anomaly
                case "UET_Concussive":
                case "UET_Shock":
                    return ModsConfig.AnomalyActive;

                default:
                    return true;
            }
        }
    }
}

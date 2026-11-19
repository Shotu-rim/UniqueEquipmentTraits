
using RimWorld;
using UnityEngine;
using Verse;
using UniqueEquipmentTraits.Effects;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Absorbs incoming pawn damage by consuming
    /// hit points of the selected apparel item.
    /// </summary>
    public class ArmorTraitWorker_Aegis :
        ArmorTraitWorker,
        IArmorDamageInterceptor
    {
        public bool CanInterceptDamage(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait,
            float damage)
        {
            if (pawn == null ||
                apparel == null ||
                apparel.Destroyed ||
                apparel.Wearer != pawn ||
                damage <= 0f)
            {
                return false;
            }

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return false;

            return quality == QualityCategory.Masterwork ||
                   quality == QualityCategory.Legendary;
        }

        public bool TryInterceptDamage(
            Pawn pawn,
            Apparel apparel,
            ArmorTraitDef trait,
            float damage)
        {
            if (!CanInterceptDamage(
                pawn,
                apparel,
                trait,
                damage))
            {
                return false;
            }

            QualityCategory quality;
            apparel.TryGetQuality(out quality);

            float reduction =
                quality == QualityCategory.Legendary
                    ? 0.80f
                    : 0.60f;

            int hpCost = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    damage * (1f - reduction)));

            DamageInfo armorDamage = new DamageInfo(
                DamageDefOf.Deterioration,
                hpCost);


            string armorName = apparel.LabelCap;
            int hpBefore = apparel.HitPoints;

            apparel.TakeDamage(armorDamage);

            int durabilityLost = Mathf.Max(
                0,
                hpBefore - (apparel.Destroyed ? 0 : apparel.HitPoints));

            string description =
    $"The aegis field of {armorName} absorbed " +
    $"{damage:0.#} damage, sacrificing " +
    $"{durabilityLost} points of armor integrity.";

            UETCombatLog.Record(
                trait,
                pawn,
                description);

            return true;

        }
    }
}

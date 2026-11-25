
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UniqueEquipmentTraits.Core.Armor;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Creates equipment for unique reward generation.
    /// Preserves the original creation behavior for non-UET items
    /// and selects a compatible random material for UET variants.
    /// Supports unique UET armor and melee weapon variants.
    /// Intended for internal reward generation and compatibility.
    /// </summary>
    internal static class UETUniqueRewardThingFactory
    {
        public static Thing MakeThing(ThingDef def, ThingDef stuff = null)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));

            // Preserve the original creation behavior for vanilla items
            if (!IsUETVariant(def))
                return ThingMaker.MakeThing(def, stuff);

            if (def.MadeFromStuff)
            {
                stuff = RandomStuffForUnique(def);

                if (stuff == null)
                {
                    throw new InvalidOperationException(
                        "[UET REWARD] No compatible stuff for " +
                        def.defName);
                }
            }

            return ThingMaker.MakeThing(def, stuff);
        }

        private static bool IsUETVariant(ThingDef def)
        {
            if (UniqueArmorInitializer.IsUniqueArmor(def))
                return true;

            // Generated UET variants use UET_Unique_ prefix
            return def.defName != null &&
                def.defName.StartsWith(
                    "UET_Unique_",
                    StringComparison.Ordinal) &&
                def.IsMeleeWeapon &&
                def.HasComp<CompUniqueWeapon>();
        }

        private static ThingDef RandomStuffForUnique(ThingDef def)
        {
            List<ThingDef> allowed = GenStuff.AllowedStuffsFor(
                def,
                TechLevel.Undefined,
                checkAllowedInStuffGeneration: true
            ).ToList();

            if (allowed.Count == 0)
                return null;

            return allowed.RandomElementByWeight(
                stuff => Mathf.Max(
                    0.01f,
                    stuff.stuffProps.commonality
                )
            );
        }
    }
}

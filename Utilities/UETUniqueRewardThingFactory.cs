
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    internal static class UETUniqueRewardThingFactory
    {
        public static Thing MakeThing(ThingDef def, ThingDef stuff = null)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));

            // Oryginalne przedmioty Odyssey:
            // zachowujemy dotychczasowy sposób tworzenia.
            if (!IsUETVariant(def))
                return ThingMaker.MakeThing(def);

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

            // Warianty UET tworzone przez generatory
            // używają prefiksu UET_Unique_.
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

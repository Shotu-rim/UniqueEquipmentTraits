
using System;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETWorldConversionThingFactory
    {
        public static bool TryCreate(
            Thing original,
            out Thing converted)
        {
            converted = null;

            if (!UETWorldConversionEligibility.IsEligible(original))
                return false;

            ThingDef variant =
                UETWorldConversionDefResolver.Resolve(original.def);

            if (variant == null)
                return false;

            CompQuality sourceQuality =
                original.TryGetComp<CompQuality>();

            if (sourceQuality == null)
                return false;

            ThingDef stuff = original.Stuff;

            // Preserve hit points proportionally if the
            // variant has a different maximum HP.
            float hpFraction = original.def.useHitPoints
                ? (float)original.HitPoints / original.MaxHitPoints
                : 1f;

            Thing replacement = null;

            try
            {
                UETWorldConversionContext.Begin(
                    sourceQuality.Quality);

                try
                {
                    replacement = ThingMaker.MakeThing(
                        variant,
                        stuff);
                }
                finally
                {
                    UETWorldConversionContext.End();
                }

                if (replacement == null)
                    return false;

                if (!HasValidTrait(replacement))
                {
                    Log.Warning(
                        "[UET WORLD] Generated variant without valid trait: " +
                        replacement.def.defName);

                    replacement.Destroy();
                    return false;
                }


                CompQuality replacementQuality =
                    replacement.TryGetComp<CompQuality>();

                if (replacementQuality == null)
                {
                    replacement.Destroy();
                    return false;
                }

                // Final safeguard: preserve source quality.
                replacementQuality.SetQuality(
                    sourceQuality.Quality,
                    ArtGenerationContext.Outsider);
                // Assign a random author to world-generated
                // unique armor, without overwriting an existing author.

                if (replacement.def.IsApparel)
                {
                    UETArtAuthorUtility.EnsureRandomAuthor(
                        replacement as ThingWithComps);
                }

                if (original.def.useHitPoints &&
                    replacement.def.useHitPoints)
                {
                    replacement.HitPoints = Math.Max(
                        1,
                        Math.Min(
                            replacement.MaxHitPoints,
                            (int)Math.Round(
                                replacement.MaxHitPoints *
                                hpFraction)));
                }

                converted = replacement;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(
                    "[UET WORLD] Conversion failed for " +
                    original.def.defName + ": " + ex);

                if (replacement != null &&
                    !replacement.Destroyed)
                {
                    replacement.Destroy();
                }

                return false;
            }
        }

        private static bool HasValidTrait(Thing thing)
        {
            if (thing == null || thing.def == null)
                return false;

            if (thing.def.IsMeleeWeapon)
            {
                CompUniqueWeapon unique =
                    thing.TryGetComp<CompUniqueWeapon>();

                if (unique == null ||
                    unique.TraitsListForReading == null ||
                    unique.TraitsListForReading.Count != 1)
                {
                    return false;
                }

                WeaponTraitDef trait =
                    unique.TraitsListForReading[0];

                return trait != null &&
                    trait.GetModExtension<
                        UniqueEquipmentTraits.Traits.UETTraitExtension>()
                        != null;
            }

            if (thing.def.IsApparel)
            {
                UniqueEquipmentTraits.Core.Armor.CompArmorTraits armor =
                    thing.TryGetComp<
                        UniqueEquipmentTraits.Core.Armor.CompArmorTraits>();

                return armor != null &&
                    armor.Traits != null &&
                    armor.Traits.Count == 1;
            }

            return false;
        }

    }
}

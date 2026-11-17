
using HarmonyLib;
using RimWorld;
using System;
using UniqueEquipmentTraits.Core;
using Verse;

namespace UniqueEquipmentTraits.Crafting
{
    /// <summary>
    /// Handles the conversion of crafted melee weapons into UET unique variants.
    /// Preserves crafting notifications and artwork events during conversion,
    /// evaluates unique crafting chances, and transfers item properties.
    /// Other mods can integrate with this system through compatible
    /// unique weapon variants and crafting definitions.
    /// </summary>
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class Patch_GenRecipe_PostProcessProduct
    {
        /// <summary>
        /// Stores temporary state for a single crafting operation.
        /// Tracks inspiration and deferred notifications while supporting
        /// nested crafting contexts on the current thread.
        /// </summary>
        internal sealed class CraftContext
        {
            internal Thing Product;
            internal CraftContext Previous;
            internal bool Inspired;
            internal bool NotificationDeferred;
            internal bool CraftedArtDeferred;
        }

        [ThreadStatic]
        private static CraftContext activeContext;

        internal static bool DeferNotification(Thing thing)
        {
            CraftContext context = activeContext;

            if (context == null ||
                !ReferenceEquals(context.Product, thing))
                return false;

            context.NotificationDeferred = true;
            return true;
        }

        internal static bool DeferCraftedArt(
            TaleDef def,
            object[] args)
        {
            CraftContext context = activeContext;

            if (context == null ||
                def != TaleDefOf.CraftedArt ||
                args == null)
                return false;

            foreach (object arg in args)
            {
                if (ReferenceEquals(arg, context.Product))
                {
                    context.CraftedArtDeferred = true;
                    return true;
                }
            }

            return false;
        }

        [HarmonyPrefix]
        private static void Prefix(
            Thing product,
            Pawn worker,
            out CraftContext __state)
        {
            __state = null;

            if (product == null || worker == null)
                return;

            Thing weapon = product;

            if (weapon is MinifiedThing minified)
                weapon = minified.InnerThing;

            if (weapon == null ||
                weapon.def == null ||
                !weapon.def.IsMeleeWeapon ||
                weapon.TryGetComp<CompUniqueWeapon>() != null ||
                weapon.TryGetComp<CompQuality>() == null)
                return;

            __state = new CraftContext
            {
                Product = weapon,
                Previous = activeContext,
                Inspired =
                    worker.InspirationDef ==
                    InspirationDefOf.Inspired_Creativity
            };

            activeContext = __state;
        }

        [HarmonyPostfix]
        private static void Postfix(
            ref Thing __result,
            RecipeDef recipeDef,
            Pawn worker,
            CraftContext __state)
        {
            if (__state == null)
                return;

            try
            {
                TryConvert(
                    ref __result,
                    recipeDef,
                    worker,
                    __state);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "[UET] Crafting processing failed: " + ex);
            }
            finally
            {
                // Restore previous context before replaying events
                activeContext = __state.Previous;

                Thing finalProduct = __result;

                if (finalProduct is MinifiedThing minified)
                    finalProduct = minified.InnerThing;

                if (finalProduct != null && worker != null)
                {
                    if (__state.CraftedArtDeferred)
                    {
                        TaleRecorder.RecordTale(
                            TaleDefOf.CraftedArt,
                            worker,
                            finalProduct);
                    }

                    if (__state.NotificationDeferred)
                    {
                        QualityUtility.SendCraftNotification(
                            finalProduct,
                            worker);
                    }
                }
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            CraftContext __state)
        {
            if (__state != null &&
                ReferenceEquals(activeContext, __state))
            {
                activeContext = __state.Previous;
            }

            return __exception;
        }

        private static void TryConvert(
            ref Thing result,
            RecipeDef recipeDef,
            Pawn worker,
            CraftContext context)
        {
            if (result == null ||
                recipeDef == null ||
                worker == null ||
                recipeDef.workSkill == null)
                return;

            Thing product = result;

            if (product is MinifiedThing minified)
            {
                // Minified products are not supported by conversion
                return;
            }

            if (product == null ||
                product.def == null ||
                !product.def.IsMeleeWeapon ||
                product.TryGetComp<CompUniqueWeapon>() != null)
                return;

            CompQuality qualityComp =
                product.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;

            int skillLevel;

            if (worker.RaceProps.IsMechanoid)
            {
                skillLevel =
                    worker.RaceProps.mechFixedSkillLevel;
            }
            else
            {
                if (worker.skills == null)
                    return;

                skillLevel =
                    worker.skills
                        .GetSkill(recipeDef.workSkill)
                        .Level;
            }

            float chance =
                UniqueCraftingChance.Calculate(
                    skillLevel,
                    qualityComp.Quality,
                    context.Inspired);

            // Roll unique crafting chance
            if (!Rand.Chance(chance))
                return;

            if (!UniqueWeaponVariantRegistry.TryGetVariant(
                    product.def,
                    out ThingDef variantDef))
            {
                Log.Warning(
                    "[UET] Missing unique variant for " +
                    product.def.defName);
                return;
            }

            Thing uniqueWeapon = null;

            try
            {
                uniqueWeapon = ThingMaker.MakeThing(
                    variantDef,
                    product.Stuff);

                CompUniqueWeapon uniqueComp =
                    uniqueWeapon.TryGetComp<CompUniqueWeapon>();

                if (uniqueComp == null ||
                    uniqueComp.TraitsListForReading == null ||
                    uniqueComp.TraitsListForReading.Count != 1)
                {
                    Log.Warning(
                        "[UET] Unique conversion failed: " +
                        "invalid trait count.");
                    return;
                }

                // Preserve non-quality properties
                uniqueWeapon.StyleDef = product.StyleDef;
                uniqueWeapon.StyleSourcePrecept =
                    product.StyleSourcePrecept;
                uniqueWeapon.overrideGraphicIndex =
                    product.overrideGraphicIndex;
                uniqueWeapon.stackCount = product.stackCount;

                CompIngredients sourceIngredients =
                    product.TryGetComp<CompIngredients>();

                CompIngredients targetIngredients =
                    uniqueWeapon.TryGetComp<CompIngredients>();

                if (sourceIngredients != null &&
                    targetIngredients != null)
                {
                    foreach (ThingDef ingredient in
                        sourceIngredients.ingredients)
                    {
                        targetIngredients.RegisterIngredient(
                            ingredient);
                    }
                }

                // Initialize artwork
                CompArt uniqueArt =
                    uniqueWeapon.TryGetComp<CompArt>();

                if (uniqueArt != null)
                {
                    uniqueArt.JustCreatedBy(worker);
                    uniqueArt.InitializeArt(
                        ArtGenerationContext.Colony);
                }

                // Use the quality generated for the unique variant
                result = uniqueWeapon;
                uniqueWeapon = null;

                bool isPrismatic =
                    uniqueComp.TraitsListForReading[0].defName ==
                    "UET_Prismatic";

                if (isPrismatic)
                {
                    Messages.Message(
                        worker.LabelShort +
                        " has achieved the impossible, forging a weapon that defies all known science!",
                        result,
                        MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message(
                        worker.LabelShort +
                        " has transcended the limits of craftsmanship, " +
                        "forging a weapon imbued with extraordinary power.",
                        result,
                        MessageTypeDefOf.PositiveEvent);
                }
            }
            finally
            {
                // Dispose of unsuccessful variants without destroying the original
                if (uniqueWeapon != null &&
                    !uniqueWeapon.Destroyed)
                {
                    uniqueWeapon.Destroy();
                }
            }
        }
    }
}

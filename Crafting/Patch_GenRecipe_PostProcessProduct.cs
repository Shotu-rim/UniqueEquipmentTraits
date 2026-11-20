
using HarmonyLib;
using RimWorld;
using System;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Crafting
{
    /// <summary>
    /// Handles the conversion of crafted melee weapons and armor into UET unique variants.
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
            internal bool EligibleForUnique;

        }

        [ThreadStatic]
        private static CraftContext activeContext;

        internal static bool DeferNotification(Thing thing)
        {
            CraftContext context = activeContext;

            if (context == null || !context.EligibleForUnique ||
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

            if (context == null || !context.EligibleForUnique ||
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

            Thing item = product;

            if (item is MinifiedThing minified)
                item = minified.InnerThing;

            if (item == null ||
                item.def == null ||
                item.TryGetComp<CompQuality>() == null)
                return;

            bool eligibleForUnique =
                (item.def.IsMeleeWeapon ||
                 item.def.apparel != null) &&
                item.TryGetComp<CompUniqueWeapon>() == null;

            __state = new CraftContext
            {
                Product = item,
                Previous = activeContext,
                Inspired =
                    worker.InspirationDef ==
                    InspirationDefOf.Inspired_Creativity,
                EligibleForUnique = eligibleForUnique
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
                TryApplyCraftsmanship(__result, worker);
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
                (!product.def.IsMeleeWeapon && product.def.apparel == null) ||
                product.TryGetComp<CompUniqueWeapon>() != null)
                return;

            bool isArmor = product.def.apparel != null;

            ThingDef variantDef;
            bool hasVariant = isArmor
                ? UniqueArmorVariantRegistry.TryGetVariant(product.def, out variantDef)
                : UniqueWeaponVariantRegistry.TryGetVariant(product.def, out variantDef);

            if (!hasVariant)
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

            Thing uniqueItem = null;

            try
            {
                uniqueItem = ThingMaker.MakeThing(
                    variantDef,
                    product.Stuff);

                CompUniqueWeapon uniqueComp =
                    uniqueItem.TryGetComp<CompUniqueWeapon>();

                if (isArmor)
                {
                    CompArmorTraits armorComp =
                        uniqueItem.TryGetComp<CompArmorTraits>();

                    if (uniqueComp == null ||
                        armorComp == null ||
                        armorComp.Traits == null ||
                        armorComp.Traits.Count == 0)
                    {
                        Log.Warning(
                            "[UET] Crafted unique armor has no armor trait: " +
                            variantDef.defName);
                        return;
                    }
                }
                else if (uniqueComp == null ||
                         uniqueComp.TraitsListForReading == null ||
                         uniqueComp.TraitsListForReading.Count != 1)
                {
                    Log.Warning(
                        "[UET] Unique weapon conversion failed: " +
                        "invalid trait count.");
                    return;
                }

                // Preserve non-quality properties
                uniqueItem.StyleDef = product.StyleDef;
                uniqueItem.StyleSourcePrecept =
                    product.StyleSourcePrecept;
                uniqueItem.overrideGraphicIndex =
                    product.overrideGraphicIndex;
                uniqueItem.stackCount = product.stackCount;

                CompIngredients sourceIngredients =
                    product.TryGetComp<CompIngredients>();

                CompIngredients targetIngredients =
                    uniqueItem.TryGetComp<CompIngredients>();

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
                    uniqueItem.TryGetComp<CompArt>();

                if (uniqueArt != null)
                {
                    uniqueArt.JustCreatedBy(worker);
                    uniqueArt.InitializeArt(
                        ArtGenerationContext.Colony);
                }

                // Use the quality generated for the unique variant
                result = uniqueItem;
                uniqueItem = null;

                if (isArmor)
                {
                    Messages.Message(
                        worker.LabelShort +
                        " has crafted a unique piece of armor imbued with extraordinary power.",
                        result,
                        MessageTypeDefOf.PositiveEvent);
                }
                else
                {
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
            }
            finally
            {
                // Dispose of unsuccessful variants without destroying the original
                if (uniqueItem != null &&
                    !uniqueItem.Destroyed)
                {
                    uniqueItem.Destroy();
                }
            }
        }

        private const bool CraftsmanshipDebugEnabled = true;

        private static void TryApplyCraftsmanship(
            Thing result,
            Pawn worker)
        {
            if (result == null || worker == null)
                return;

            Thing product = result;

            if (product is MinifiedThing minified)
                product = minified.InnerThing;

            if (product == null)
                return;

            CompQuality qualityComp =
                product.TryGetComp<CompQuality>();

            if (qualityComp == null)
                return;


            float chance =
                ArmorTraitCraftsmanshipUtility
                    .GetUpgradeChance(worker);

            if (CraftsmanshipDebugEnabled)
            {
                Log.Message(
                    "[UET Craftsmanship DEBUG] " +
                    "Pawn: " + worker.LabelShort +
                    " | Product: " + product.Label +
                    " | Detected chance: " +
                    (chance * 100f).ToString("0.##") + "%");
            }

            if (chance <= 0f)
                return;


            QualityCategory originalQuality =
                qualityComp.Quality;

            if (originalQuality >= QualityCategory.Legendary)
            {
                if (CraftsmanshipDebugEnabled)
                {
                    Log.Message(
                        "[UET Craftsmanship]\n" +
                        "Pawn: " + worker.LabelShort + "\n" +
                        "Product: " + product.Label + "\n" +
                        "Original quality: " + originalQuality + "\n" +
                        "Chance: " + (chance * 100f).ToString("0.##") + "%\n" +
                        "Result: SKIPPED (maximum quality)");
                }

                return;
            }

            float roll = Rand.Value;
            bool success = roll < chance;

            if (success)
            {
                QualityCategory upgradedQuality =
                    (QualityCategory)((int)originalQuality + 1);

                qualityComp.SetQuality(
                    upgradedQuality,
                    ArtGenerationContext.Colony);
            }

            if (CraftsmanshipDebugEnabled)
            {
                Log.Message(
                    "[UET Craftsmanship]\n" +
                    "Pawn: " + worker.LabelShort + "\n" +
                    "Product: " + product.Label + "\n" +
                    "Original quality: " + originalQuality + "\n" +
                    "Chance: " + (chance * 100f).ToString("0.##") + "%\n" +
                    "Roll: " + (roll * 100f).ToString("0.00") + "%\n" +
                    "Result: " + (success ? "SUCCESS" : "FAILED") + "\n" +
                    "Final base quality: " + qualityComp.Quality);
            }
        }

    }
}

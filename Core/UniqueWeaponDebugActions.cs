
using LudeonTK;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class UniqueWeaponDebugActions
    {
        [DebugAction(
    "Unique Equipment Traits",
    "Test unique weapon variant",
    actionType = DebugActionType.Action)]
        public static void TestUniqueWeaponVariant()
        {
            Thing weapon = null;

            try
            {
                ThingDef original =
    DefDatabase<ThingDef>.AllDefs
        .FirstOrDefault(def =>
        {
            ThingDef variant;

            return def.IsMeleeWeapon &&
                UniqueWeaponVariantRegistry
                    .TryGetVariant(def, out variant);
        });

                if (original == null)
                {
                    Log.Warning(
                        "[UET TEST] No registered weapon variants.");
                    return;
                }

                ThingDef variantDef;

                if (!UniqueWeaponVariantRegistry.TryGetVariant(
                    original, out variantDef))
                {
                    Log.Error(
                        "[UET TEST] Variant lookup failed.");
                    return;
                }

                ThingDef stuff = null;

                if (variantDef.MadeFromStuff)
                {
                    stuff = GenStuff.DefaultStuffFor(variantDef);

                    if (stuff == null)
                    {
                        Log.Error(
                            "[UET TEST] No valid material for " +
                            variantDef.defName);
                        return;
                    }
                }

                Log.Message(
                    "[UET TEST] Original: " +
                    original.defName +
                    " | Variant: " +
                    variantDef.defName);

                weapon = ThingMaker.MakeThing(
                    variantDef, stuff);

                ThingWithComps weaponWithComps =
                    weapon as ThingWithComps;

                if (weaponWithComps == null)
                {
                    Log.Error(
                        "[UET TEST] Created thing has no comps.");
                    return;
                }

                CompUniqueWeapon uniqueComp =
                    weaponWithComps.TryGetComp<CompUniqueWeapon>();

                if (uniqueComp == null)
                {
                    Log.Error(
                        "[UET TEST] CompUniqueWeapon missing.");
                    return;
                }

                var traits = uniqueComp.TraitsListForReading;

                Log.Message(
                    "[UET TEST] CompUniqueWeapon found.");

                if (traits == null)
                {
                    Log.Warning(
                        "[UET TEST] Trait list is null.");
                    return;
                }

                Log.Message(
                    "[UET TEST] Trait count: " +
                    traits.Count);

                foreach (RimWorld.WeaponTraitDef trait in traits)
                {
                    Log.Message(
                        "[UET TEST] Trait: " +
                        (trait != null
                            ? trait.defName
                            : "NULL"));
                }

                CompQuality qualityComp =
                    weaponWithComps.TryGetComp<CompQuality>();

                if (qualityComp != null)
                {
                    Log.Message(
                        "[UET TEST] Quality: " +
                        qualityComp.Quality);
                }

                Log.Message(
                    "[UET TEST] Completed.");
            }
            catch (Exception exception)
            {
                Log.Error(
                    "[UET TEST] Exception: " +
                    exception);
            }
            finally
            {
                if (weapon != null &&
                    !weapon.Destroyed)
                {
                    weapon.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}

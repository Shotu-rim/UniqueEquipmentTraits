
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(
        typeof(CompUniqueWeapon),
        "InitializeTraits")]
    internal static class Patch_CompUniqueWeapon_DevInitializeTraits
    {
        [HarmonyPrefix]
        private static bool Prefix(
            CompUniqueWeapon __instance)
        {
            ThingWithComps weapon =
                __instance?.parent as ThingWithComps;

            if (!UETDevSpawnContext.Matches(weapon))
                return true;

            WeaponTraitDef selectedTrait =
                UETDevSpawnContext.SelectedTrait;

            if (selectedTrait == null)
                return true;

            // The dev spawner supplies the trait directly.
            // Do not run Odyssey's random selection.
            if (!__instance.CanAddTrait(selectedTrait))
            {
                Log.Error(
                    "[UET] Dev spawn rejected trait " +
                    selectedTrait.defName +
                    " for " + weapon.def.defName);

                return false;
            }

            __instance.AddTrait(selectedTrait);
            __instance.Setup(fromSave: false);

            return false;
        }
    }
}

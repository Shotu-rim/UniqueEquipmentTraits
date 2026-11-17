
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Overrides Odyssey trait initialization for weapons created
    /// through the UET development spawner.
    /// Assigns the selected weapon trait after validating compatibility
    /// and initializes the unique weapon component without running
    /// the standard randomized trait initialization.
    /// </summary>
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

            // Dev spawner tool generates item directly without Odyssey
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

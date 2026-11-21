
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Initializes the UET mod, registers Harmony patches,
    /// and schedules armor trait component injection.
    /// Also registers a patch for generating UET equipment
    /// definitions during RimWorld's implied definition phase.
    /// </summary>
    public class ModInitializer : Mod
    {
        public ModInitializer(ModContentPack content)
            : base(content)
        {
            Log.Warning("[UET TEST] ModInitializer constructor executed!");
            var harmony = new Harmony("shotu.uniqueequipmenttraits");
            harmony.PatchAll();
            VPEVortexCompatibility.Apply(harmony);
            VPEColdImmunityCompatibility.Apply(harmony);
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ArmorTraitCompInjector.Inject();
                LogCapacityPatches();
            });
        }


        private static void LogCapacityPatches()
        {
            Log.Warning("[UET DEBUG] Diagnostics STARTED");

            var method = AccessTools.Method(
                typeof(PawnCapacityUtility),
                nameof(PawnCapacityUtility.CalculateCapacityLevel));

            if (method == null)
            {
                Log.Warning("[UET DEBUG] Method NOT FOUND");
                return;
            }

            Log.Warning("[UET DEBUG] Method FOUND");

            var patchInfo = Harmony.GetPatchInfo(method);

            if (patchInfo == null)
            {
                Log.Warning("[UET DEBUG] No Harmony patches");
                return;
            }

            Log.Warning(
                "[UET DEBUG] Postfix count: " +
                patchInfo.Postfixes.Count);

            foreach (var patch in patchInfo.Postfixes)
            {
                Log.Warning(
                    "[UET DEBUG] Owner: " + patch.owner +
                    " | Patch: " + patch.PatchMethod.FullDescription());
            }
        }


        /// <summary>
        /// Injects weapon trait definitions and generates UET
        /// equipment variants during implied definition generation.
        /// </summary>
        [HarmonyPatch(
            typeof(DefGenerator),
            nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
        public static class Patch_ImpliedDefs
        {
            [HarmonyPostfix]
            public static void Postfix(bool hotReload)
            {
                WeaponTraitDefInjector.Inject();
                UniqueWeaponDefGenerator.Generate(hotReload);
                UniqueArmorTestDefGenerator.Generate(hotReload);
            }
        }
    }
}


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
            var harmony = new Harmony("shotu.uniqueequipmenttraits");
            harmony.PatchAll();
            VPEVortexCompatibility.Apply(harmony);
            VPEColdImmunityCompatibility.Apply(harmony);
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ArmorTraitCompInjector.Inject();
            });
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
                HemostasisHediffCompInjector.Inject();
            }
        }
    }
}

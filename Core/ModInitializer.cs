
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public class ModInitializer : Mod
    {
        public ModInitializer(ModContentPack content)
            : base(content)
        {
            var harmony = new Harmony("shotu.uniqueequipmenttraits");
            harmony.PatchAll();

            UniqueEquipmentTraits.Patches
    .Patch_ThingSetMaker_UniqueWeapon_ArmorPool
    .LogPatchStatus();

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ArmorTraitCompInjector.Inject();
               
            });
        }
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

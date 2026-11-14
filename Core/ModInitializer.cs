
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

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ArmorTraitCompInjector.Inject();
                ArmorTraitDiagnostics.Run();
          //      Log.Message("[UET ARMOR TEST] TEST STARTED");

                

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
                Log.Message("[UET] Patch_ImpliedDefs Postfix CALLED");

                WeaponTraitDefInjector.Inject();

                Log.Message("[UET] Starting unique variant generator");

                UniqueWeaponDefGenerator.Generate(hotReload);

                Log.Message("[UET] Unique variant generator FINISHED");
            }
        }

        [StaticConstructorOnStartup]
        public static class UETDiagnostics
        {
            static UETDiagnostics()
            {


                int meleeWeapons = 0;
                int odysseyWeapons = 0;

                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (!def.IsMeleeWeapon ||
                        def.weaponTags == null ||
                        def.weaponTags.Count == 0)
                    {
                        continue;
                    }

                    meleeWeapons++;

                    if (def.HasComp<RimWorld.CompUniqueWeapon>())
                    {
                        odysseyWeapons++;
                    }
                }



            }
        }

    }
}

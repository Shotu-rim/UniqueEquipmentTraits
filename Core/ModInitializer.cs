
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
                var candidates =
    UniqueArmorCandidateFinder.GetEligibleApparel();

                int stuffCount = 0;
                int nonStuffCount = 0;

                foreach (ThingDef def in candidates)
                {
                    if (def.MadeFromStuff)
                        stuffCount++;
                    else
                        nonStuffCount++;
                }

                Log.Message(
                    "[UET ARMOR UNIQUE] Eligible apparel: " +
                    candidates.Count +
                    " | Stuff-based: " + stuffCount +
                    " | Non-stuff: " + nonStuffCount);
                ArmorTraitDiagnostics.Run();
                ThingDef testArmor =
            DefDatabase<ThingDef>.GetNamedSilentFail(
                "UET_Unique_Test_Armor");

                if (testArmor != null)
                {
                    bool eligible =
                        ArmorTraitEligibility.CanReceiveArmorTrait(
                            testArmor);

                    Log.Message(
                        "[UET ARMOR UNIQUE TEST] Post-load eligibility: " +
                        eligible +
                        " | Original: Apparel_AirwireHeadset" +
                        " | Has CompUniqueWeapon: " +
                        testArmor.HasComp<CompUniqueWeapon>() +
                        " | Has CompArmorTraits: " +
                        testArmor.HasComp<CompArmorTraits>());
                }



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
                UniqueArmorTestDefGenerator.Generate(hotReload);

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

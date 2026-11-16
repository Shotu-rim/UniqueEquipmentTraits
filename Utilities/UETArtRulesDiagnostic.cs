
//using System.Linq;
//using RimWorld;
//using Verse;

//namespace UniqueEquipmentTraits.Utilities
//{
//    [StaticConstructorOnStartup]
//    public static class UETArtRulesDiagnostic
//    {
//        static UETArtRulesDiagnostic()
//        {
//            LongEventHandler.ExecuteWhenFinished(
//                LogOdysseyArtRules);
//        }

//        private static void LogOdysseyArtRules()
//        {
//            Log.Message(
//                "[UET ART RULES] Starting Odyssey weapon scan.");

//            int found = 0;

//            foreach (ThingDef def in
//                DefDatabase<ThingDef>.AllDefs)
//            {
//                if (!def.IsWeapon ||
//                    !def.IsRangedWeapon ||
//                    def.comps == null)
//                {
//                    continue;
//                }

//                CompProperties_UniqueWeapon unique =
//                    def.comps
//                        .OfType<CompProperties_UniqueWeapon>()
//                        .FirstOrDefault();

//                if (unique == null)
//                    continue;

//                CompProperties_Art art =
//                    def.comps
//                        .OfType<CompProperties_Art>()
//                        .FirstOrDefault();

//                found++;

//                Log.Message(
//                    "\n========== UET ODYSSEY ART RULES ==========\n" +
//                    "Weapon: " + def.label + "\n" +
//                    "DefName: " + def.defName + "\n" +
//                    "Has CompArt: " + (art != null) + "\n" +
//                    "NameMaker: " +
//                    (art != null && art.nameMaker != null
//                        ? art.nameMaker.defName
//                        : "(none)") + "\n" +
//                    "DescriptionMaker: " +
//                    (art != null && art.descriptionMaker != null
//                        ? art.descriptionMaker.defName
//                        : "(none)") + "\n" +
//                    "MinQualityForArtistic: " +
//                    (art != null
//                        ? art.minQualityForArtistic.ToString()
//                        : "(none)") + "\n" +
//                    "===========================================");
//            }

//            Log.Message(
//                "[UET ART RULES] Scan finished. " +
//                "Unique ranged weapon definitions found: " +
//                found);
//        }
//    }
//}


using System.Linq;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [StaticConstructorOnStartup]
    public static class UETArtRulesDiagnostic
    {
        static UETArtRulesDiagnostic()
        {
            LongEventHandler.ExecuteWhenFinished(LogApparelArtRules);
        }

        private static void LogApparelArtRules()
        {
            Log.Message("[UET APPAREL ART] Starting apparel scan.");

            int apparelCount = 0;
            int withArt = 0;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.category != ThingCategory.Item ||
                    !def.IsApparel)
                {
                    continue;
                }

                apparelCount++;

                CompProperties_Art art = def.comps?
                    .OfType<CompProperties_Art>()
                    .FirstOrDefault();

                if (art == null)
                    continue;

                withArt++;

                Log.Message(
                    "[UET APPAREL ART]\n" +
                    "Item: " + def.label + "\n" +
                    "DefName: " + def.defName + "\n" +
                    "NameMaker: " +
                    (art.nameMaker != null
                        ? art.nameMaker.defName
                        : "(none)") + "\n" +
                    "DescriptionMaker: " +
                    (art.descriptionMaker != null
                        ? art.descriptionMaker.defName
                        : "(none)") + "\n" +
                    "MinQualityForArtistic: " +
                    art.minQualityForArtistic);
            }

            Log.Message(
                "[UET APPAREL ART] Scan finished. " +
                "Apparel definitions: " + apparelCount +
                " | With CompArt: " + withArt);
        }
    }
}


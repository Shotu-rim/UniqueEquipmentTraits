
using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    [HarmonyPatch(
        typeof(ThingSetMaker),
        nameof(ThingSetMaker.Generate),
        new Type[] { typeof(ThingSetMakerParams) })]
    public static class Patch_ThingSetMaker_Generate_UETDiagnostics
    {
        private static int nextOperationId;

        [HarmonyPrefix]
        public static void Prefix(
            ThingSetMaker __instance,
            out int __state)
        {
            __state = ++nextOperationId;

            if (!Prefs.DevMode)
                return;

            Log.Message(
                "[UET][WORLD][" + __state + "][START] " +
                "Generator: " + __instance.GetType().FullName);
        }

        [HarmonyPostfix]
        public static void Postfix(
            ThingSetMaker __instance,
            List<Thing> __result,
            int __state)
        {
            if (!Prefs.DevMode)
                return;

            string source = __instance.GetType().Name;

            if (__result == null)
            {
                Log.Warning(
                    "[UET][WORLD][" + __state +
                    "][RESULT] NULL | Source: " + source);
                return;
            }

            Log.Message(
                "[UET][WORLD][" + __state +
                "][RESULT] Source: " + source +
                " | Count: " + __result.Count);

            int candidates = 0;
            int skipped = 0;

            for (int i = 0; i < __result.Count; i++)
            {
                Thing thing = __result[i];

                if (thing == null)
                {
                    Log.Warning(
                        "[UET][WORLD][" + __state +
                        "][ITEM " + i + "] NULL");
                    skipped++;
                    continue;
                }

                bool minified = thing is MinifiedThing;

                Thing inspected = thing;

                if (thing is MinifiedThing wrapper)
                    inspected = wrapper.InnerThing;

                if (inspected == null || inspected.def == null)
                {
                    Log.Warning(
                        "[UET][WORLD][" + __state +
                        "][ITEM " + i +
                        "] Missing inner Thing or ThingDef");
                    skipped++;
                    continue;
                }

                ThingDef def = inspected.def;

                CompQuality qualityComp =
                    inspected.TryGetComp<CompQuality>();

                CompUniqueWeapon uniqueComp =
                    inspected.TryGetComp<CompUniqueWeapon>();

                bool hasQuality = qualityComp != null;
                bool alreadyUnique = uniqueComp != null;

                bool melee = def.IsMeleeWeapon;
                bool apparel = def.IsApparel;

                bool supportedType = melee || apparel;

                QualityCategory quality =
                    hasQuality
                        ? qualityComp.Quality
                        : QualityCategory.Awful;

                bool eligibleQuality =
                    hasQuality &&
                    (quality == QualityCategory.Masterwork ||
                     quality == QualityCategory.Legendary);

                Log.Message(
                    "[UET][WORLD][" + __state +
                    "][ITEM " + i + "]" +
                    " Def: " + def.defName +
                    " | Stuff: " +
                    (inspected.Stuff != null
                        ? inspected.Stuff.defName
                        : "none") +
                    " | Quality: " +
                    (hasQuality
                        ? quality.ToString()
                        : "none") +
                    " | Minified: " + minified +
                    " | Melee: " + melee +
                    " | Apparel: " + apparel +
                    " | CompUniqueWeapon: " + alreadyUnique);

                string reason;

                if (alreadyUnique)
                {
                    reason = "ALREADY_UNIQUE";
                }
                else if (!supportedType)
                {
                    reason = "UNSUPPORTED_TYPE";
                }
                else if (!hasQuality)
                {
                    reason = "NO_QUALITY_COMP";
                }
                else if (!eligibleQuality)
                {
                    reason = "QUALITY_BELOW_MASTERWORK";
                }
                else
                {
                    reason = "PRELIMINARY_CANDIDATE";
                }

                if (reason == "PRELIMINARY_CANDIDATE")
                    candidates++;
                else
                    skipped++;

                Log.Message(
                    "[UET][WORLD][" + __state +
                    "][CHECK " + i + "] " +
                    "Result: " + reason);
            }

            Log.Message(
                "[UET][WORLD][" + __state +
                "][END] Source: " + source +
                " | Items: " + __result.Count +
                " | PreliminaryCandidates: " + candidates +
                " | Skipped: " + skipped);
        }
    }
}

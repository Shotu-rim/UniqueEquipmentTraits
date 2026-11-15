
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UniqueEquipmentTraits.Core.Armor;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch]
    internal static class Patch_ThingSetMaker_UniqueWeapon_ArmorPool
    {

        public static void LogPatchStatus()
        {
            MethodInfo target = AccessTools.Method(
                typeof(ThingSetMaker_UniqueWeapon),
                "Generate",
                new[]
                {
            typeof(ThingSetMakerParams),
            typeof(List<Thing>)
                });

            if (target == null)
            {
                Log.Error("[UET PATCH CHECK] Generate method NOT FOUND");
                return;
            }

            var info = Harmony.GetPatchInfo(target);

            if (info == null)
            {
                Log.Error("[UET PATCH CHECK] No Harmony patches registered");
                return;
            }

            Log.Message("[UET PATCH CHECK] Generate patches:");

            foreach (var patch in info.Prefixes)
            {
                Log.Message(
                    "[UET PATCH CHECK] PREFIX: " +
                    patch.owner + " | " + patch.PatchMethod);
            }

            foreach (var patch in info.Transpilers)
            {
                Log.Message(
                    "[UET PATCH CHECK] TRANSPILER: " +
                    patch.owner + " | " + patch.PatchMethod);
            }
        }


        private static readonly MethodInfo WhereMethod =
            typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method =>
                    method.Name == nameof(Enumerable.Where) &&
                    method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 2 &&
                    method.GetParameters()[1].ParameterType
                        .GetGenericArguments().Length == 2)
                .Single()
                .MakeGenericMethod(typeof(ThingDef));


        private static readonly MethodInfo FilterMethod =
            AccessTools.Method(
                typeof(Patch_ThingSetMaker_UniqueWeapon_ArmorPool),
                nameof(FilterCandidates));


        private static IEnumerable<ThingDef> FilterCandidates(
            IEnumerable<ThingDef> candidates)
        {
            Log.Message(
                "[UET ARMOR POOL] FilterCandidates CALLED");

            List<ThingDef> filtered = candidates
                .Where(UniqueArmorPoolFilter.CanGenerate)
                .ToList();

            Log.Message(
                "[UET ARMOR POOL] Candidates after filtering: " +
                filtered.Count);

            return filtered;
        }


        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            Type type = typeof(ThingSetMaker_UniqueWeapon);

            yield return AccessTools.Method(
                type,
                "Generate",
                new[]
                {
            typeof(ThingSetMakerParams),
            typeof(List<Thing>)
                });

            yield return AccessTools.Method(
                type,
                "AllGeneratableThingsDebugSub",
                new[]
                {
            typeof(ThingSetMakerParams)
                });
        }


        [HarmonyPrefix]
        private static void Prefix(MethodBase __originalMethod)
        {
            ThingSetMakerDef def = ThingSetMakerDefOf.Reward_UniqueWeapon;

            Log.Message(
                "[UET REWARD CHECK] " +
                def.defName +
                " | root: " +
                def.root.GetType().FullName);

            Log.Message(
                "[UET ARMOR POOL] Entered: " +
                __originalMethod.Name);
        }


        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            List<CodeInstruction> codes =
                instructions.ToList();




            int matches = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(WhereMethod))
                {
                    matches++;
                }
            }

            if (matches != 1)
            {
                Log.Error(
                    "[UET ARMOR UNIQUE] Unexpected LINQ layout in " +
                    original.Name +
                    ". Expected one Where call, found " +
                    matches + ". Original method preserved.");

                return codes;
            }

            List<CodeInstruction> result =
                new List<CodeInstruction>();

            foreach (CodeInstruction instruction in codes)
            {
                result.Add(instruction);

                if (instruction.Calls(WhereMethod))
                {
                    result.Add(
                        new CodeInstruction(
                            OpCodes.Call,
                            FilterMethod));
                }
            }

            Log.Message(
                "[UET ARMOR UNIQUE] Pool filter patched: " +
                original.Name);

            return result;
        }
    }
}

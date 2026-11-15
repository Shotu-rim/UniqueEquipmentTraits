
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
            return candidates.Where(
                UniqueArmorPoolFilter.CanGenerate);
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

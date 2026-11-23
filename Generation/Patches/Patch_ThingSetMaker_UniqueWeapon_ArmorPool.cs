
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Core.Armor;
using UniqueEquipmentTraits.Utilities;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Integrates UET equipment generation with Odyssey's unique
    /// weapon generator.
    /// Filters candidate definitions using UET armor pool rules
    /// and redirects item creation to the UET reward factory.
    /// Validates expected IL patterns before applying changes
    /// to preserve the original methods if their layout differs.
    /// </summary>
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

        private static readonly MethodInfo OriginalMakeThingMethod =
            AccessTools.Method(
                typeof(ThingMaker),
                nameof(ThingMaker.MakeThing),
                new[]
                {
                    typeof(ThingDef),
                    typeof(ThingDef)
                });

        private static readonly MethodInfo UETMakeThingMethod =
            AccessTools.Method(
                typeof(UETUniqueRewardThingFactory),
                nameof(UETUniqueRewardThingFactory.MakeThing),
                new[]
                {
                    typeof(ThingDef),
                    typeof(ThingDef)
                });

        private static IEnumerable<ThingDef> FilterCandidates(
            IEnumerable<ThingDef> candidates)
        {
            return candidates
                .Where(UniqueArmorPoolFilter.CanGenerate)
                .ToList();
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
                    matches +
                    ". Original method preserved.");

                return codes;
            }

            bool isGenerate = original.Name == "Generate";

            int makeThingMatches = isGenerate
                ? codes.Count(code =>
                    code.Calls(OriginalMakeThingMethod))
                : 0;

            if (isGenerate && makeThingMatches != 1)
            {
                Log.Error(
                    "[UET REWARD] Expected one ThingMaker.MakeThing call, found " +
                    makeThingMatches +
                    ". Original method preserved.");

                return codes;
            }

            List<CodeInstruction> result =
                new List<CodeInstruction>();

            foreach (CodeInstruction instruction in codes)
            {
                if (isGenerate &&
                    instruction.Calls(OriginalMakeThingMethod))
                {
                    CodeInstruction replacement =
                        new CodeInstruction(instruction);

                    replacement.opcode = OpCodes.Call;
                    replacement.operand = UETMakeThingMethod;

                    result.Add(replacement);
                    continue;
                }

                result.Add(instruction);

                if (instruction.Calls(WhereMethod))
                {
                    result.Add(
                        new CodeInstruction(
                            OpCodes.Call,
                            FilterMethod));
                }
            }

            return result;
        }
    }
}

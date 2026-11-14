
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(typeof(CompUniqueWeapon), "InitializeTraits")]
    public static class Patch_CompUniqueWeapon_InitializeTraits
    {
        private static readonly MethodInfo OriginalGetter =
            AccessTools.PropertyGetter(
                typeof(IntRange),
                "RandomInRange");

        private static readonly MethodInfo ReplacementMethod =
            AccessTools.Method(
                typeof(Patch_CompUniqueWeapon_InitializeTraits),
                nameof(GetTraitCount));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!patched &&
                    OriginalGetter != null &&
                    instruction.Calls(OriginalGetter))
                {
                    // Stack contains an IntRange address. Add CompUniqueWeapon
                    yield return new CodeInstruction(OpCodes.Ldarg_0);

                    CodeInstruction replacement =
                        new CodeInstruction(
                            OpCodes.Call,
                            ReplacementMethod);

                    replacement.labels.AddRange(instruction.labels);
                    replacement.blocks.AddRange(instruction.blocks);

                    yield return replacement;

                    patched = true;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (patched)
            {

            }
            else
            {
                Log.Error(
                    "[UET] Could not patch unique weapon trait count!");
            }
        }

        private static int GetTraitCount(
            ref IntRange range,
            CompUniqueWeapon uniqueComp)
        {
            Thing parent = uniqueComp?.parent;

            if (parent?.def?.defName != null &&
                parent.def.defName.StartsWith("UET_Unique_") &&
                parent.def.defName.EndsWith("_Weapon"))
            {
                return 1;
            }

            // Preserve Odyssey behavior
            return range.RandomInRange;
        }
    }
}

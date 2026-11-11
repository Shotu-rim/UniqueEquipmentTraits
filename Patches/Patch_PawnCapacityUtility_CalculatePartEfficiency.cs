
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using UniqueEquipmentTraits.Implants;

namespace UniqueEquipmentTraits.Patches
{
    [HarmonyPatch(
        typeof(PawnCapacityUtility),
        nameof(PawnCapacityUtility.CalculatePartEfficiency))]
    public static class Patch_PawnCapacityUtility_CalculatePartEfficiency
    {

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var original = new List<CodeInstruction>(instructions);

            FieldInfo defField =
                AccessTools.Field(typeof(Hediff), "def");

            FieldInfo propsField =
                AccessTools.Field(typeof(HediffDef), "addedPartProps");

            FieldInfo efficiencyField =
                AccessTools.Field(
                    typeof(AddedBodyPartProps),
                    "partEfficiency");

            MethodInfo replacement =
                AccessTools.Method(
                    typeof(NaniteImplantUtility),
                    nameof(NaniteImplantUtility.GetEffectivePartEfficiency));

            var matches = new List<int>();

            for (int i = 0; i < original.Count - 3; i++)
            {
                if (IsLoadLocal(original[i]) &&
                    original[i + 1].LoadsField(defField) &&
                    original[i + 2].LoadsField(propsField) &&
                    original[i + 3].LoadsField(efficiencyField))
                {
                    matches.Add(i);
                }
            }

            if (matches.Count != 3)
            {
                Log.Error(
                    $"[UET] Nanite transpiler: expected 3 matches, " +
                    $"found {matches.Count}. Original IL preserved."
                );

                return original;
            }

            // Nie usuwamy instrukcji z etykietami ani
            // granicami bloków wyjątków.
            foreach (int index in matches)
            {
                for (int offset = 1; offset <= 3; offset++)
                {
                    CodeInstruction instruction = original[index + offset];

                    if (instruction.labels.Count > 0 ||
                        instruction.blocks.Count > 0)
                    {
                        Log.Error(
                            "[UET] Nanite transpiler: unsafe IL metadata. " +
                            "Original IL preserved."
                        );

                        return original;
                    }
                }
            }

            var codes = new List<CodeInstruction>(original);

            // Przetwarzamy od końca, aby usuwanie instrukcji
            // nie zmieniało indeksów wcześniejszych dopasowań.
            for (int m = matches.Count - 1; m >= 0; m--)
            {
                int index = matches[m];

                codes[index + 1] =
                    new CodeInstruction(OpCodes.Call, replacement);

                codes.RemoveRange(index + 2, 2);
            }

            Log.Message(
                "[UET] Nanite transpiler: 3 efficiency reads patched."
            );

            return codes;
        }

        private static bool IsLoadLocal(CodeInstruction instruction)
        {
            OpCode opcode = instruction.opcode;

            return opcode == OpCodes.Ldloc ||
                   opcode == OpCodes.Ldloc_S ||
                   opcode == OpCodes.Ldloc_0 ||
                   opcode == OpCodes.Ldloc_1 ||
                   opcode == OpCodes.Ldloc_2 ||
                   opcode == OpCodes.Ldloc_3;
        }

    }



}
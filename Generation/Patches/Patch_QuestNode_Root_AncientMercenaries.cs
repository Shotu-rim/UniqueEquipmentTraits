using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UniqueEquipmentTraits.Utilities;
using Verse;

namespace UniqueEquipmentTraits.Generation.Patches
{
    [HarmonyPatch(
        typeof(QuestNode_Root_AncientMercenaries),
        "RunInt")]
    public static class Patch_QuestNode_Root_AncientMercenaries
    {
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            MethodInfo destroyEquipment = AccessTools.Method(
                typeof(Pawn_EquipmentTracker),
                nameof(Pawn_EquipmentTracker.DestroyAllEquipment),
                new[] { typeof(DestroyMode) });

            MethodInfo addEquipment = AccessTools.Method(
                typeof(Pawn_EquipmentTracker),
                nameof(Pawn_EquipmentTracker.AddEquipment),
                new[] { typeof(ThingWithComps) });

            MethodInfo assignEquipment = AccessTools.Method(
                typeof(UETUniqueEquipmentUtility),
                nameof(UETUniqueEquipmentUtility.Assign));

            if (destroyEquipment == null ||
                addEquipment == null ||
                assignEquipment == null)
            {
                return Fail(codes, "Required method not found.");
            }

            List<int> destroyCalls = FindCalls(codes, destroyEquipment);
            List<int> addCalls = FindCalls(codes, addEquipment);

            if (destroyCalls.Count != 1 || addCalls.Count != 1)
            {
                return Fail(codes, "Unexpected equipment call count.");
            }

            int destroyIndex = destroyCalls[0];
            int addIndex = addCalls[0];

            // Expected vanilla equipment assignment block:
            //
            // ldloc equipment
            // brfalse exit
            // ldloc pawn
            // ldfld Pawn.equipment
            // ldc.i4.0
            // callvirt DestroyAllEquipment
            // ldloc pawn
            // ldfld Pawn.equipment
            // ldloc equipment
            // callvirt AddEquipment

            int assignmentStart = destroyIndex - 5;

            if (assignmentStart < 0 ||
                addIndex != destroyIndex + 4 ||
                addIndex + 1 >= codes.Count)
            {
                return Fail(codes, "Unexpected assignment block.");
            }

            FieldInfo equipmentField = AccessTools.Field(
                typeof(Pawn),
                nameof(Pawn.equipment));

            if (!codes[assignmentStart].IsLdloc() ||
                (codes[assignmentStart + 1].opcode != OpCodes.Brfalse &&
                 codes[assignmentStart + 1].opcode != OpCodes.Brfalse_S) ||
                !codes[assignmentStart + 2].IsLdloc() ||
                !codes[assignmentStart + 3].LoadsField(equipmentField) ||
                codes[assignmentStart + 4].opcode != OpCodes.Ldc_I4_0 ||
                !codes[destroyIndex + 1].IsLdloc() ||
                !codes[destroyIndex + 2].LoadsField(equipmentField) ||
                !codes[destroyIndex + 3].IsLdloc())
            {
                return Fail(codes, "Assignment pattern mismatch.");
            }

            if (!(codes[assignmentStart + 1].operand is Label exitLabel) ||
                !codes[addIndex + 1].labels.Contains(exitLabel))
            {
                return Fail(codes, "Unexpected assignment branch target.");
            }

            // Locate Enumerable.First<Thing>()

            List<int> firstCalls = new List<int>();

            for (int i = 0; i < assignmentStart; i++)
            {
                if (codes[i].operand is MethodInfo method &&
                    method.DeclaringType == typeof(Enumerable) &&
                    method.Name == nameof(Enumerable.First) &&
                    method.IsGenericMethod &&
                    method.GetGenericArguments().Length == 1 &&
                    method.GetGenericArguments()[0] == typeof(Thing) &&
                    method.GetParameters().Length == 1)
                {
                    firstCalls.Add(i);
                }
            }

            if (firstCalls.Count != 1)
            {
                return Fail(codes, "Unexpected reward selection call count.");
            }

            int firstIndex = firstCalls[0];

            if (firstIndex < 1 ||
                firstIndex + 2 >= codes.Count ||
                !codes[firstIndex - 1].IsLdloc() ||
                codes[firstIndex + 1].opcode != OpCodes.Isinst ||
                !Equals(
                    codes[firstIndex + 1].operand,
                    typeof(ThingWithComps)) ||
                !codes[firstIndex + 2].IsStloc())
            {
                return Fail(codes, "Reward selection pattern mismatch.");
            }

            // Locate ThingSetMaker.Generate(parms) and its result

            MethodInfo generate = AccessTools.Method(
                typeof(ThingSetMaker),
                nameof(ThingSetMaker.Generate),
                new[] { typeof(ThingSetMakerParams) });

            List<int> generateCalls = FindCalls(codes, generate);

            if (generateCalls.Count != 1)
            {
                return Fail(codes, "Unexpected reward generator call count.");
            }

            int generateIndex = generateCalls[0];

            if (generateIndex < 1 ||
                generateIndex + 1 >= firstIndex ||
                !codes[generateIndex - 1].IsLdloc() ||
                !codes[generateIndex + 1].IsStloc())
            {
                return Fail(codes, "Reward generator pattern mismatch.");
            }

            int selectionStart = generateIndex + 2;
            int selectionEnd = firstIndex + 2;

            if (selectionStart > selectionEnd ||
                selectionEnd >= assignmentStart)
            {
                return Fail(codes, "Unexpected reward selection boundaries.");
            }

            // Check local variable operands from the original IL

            CodeInstruction loadPawn =
                CopyInstruction(codes[assignmentStart + 2]);

            CodeInstruction loadRewards =
                CopyInstruction(codes[firstIndex - 1]);

            CodeInstruction loadParms =
                CopyInstruction(codes[generateIndex - 1]);

            CodeInstruction storeEquipment =
                CopyInstruction(codes[firstIndex + 2]);

            // Do not remove unexpected exception blocks; dragons might be here

            if (!CanReplaceRange(
                    codes,
                    assignmentStart,
                    addIndex) ||
                !CanReplaceRange(
                    codes,
                    selectionStart,
                    selectionEnd))
            {
                return Fail(codes, "Unsafe IL replacement boundaries.");
            }

            // Replace equipment assignment first to preserve earlier list

            List<CodeInstruction> assignment = new List<CodeInstruction>
            {
                loadPawn,
                loadRewards,
                loadParms,
                new CodeInstruction(OpCodes.Call, assignEquipment),
                CopyInstruction(storeEquipment)
            };

            assignment[0].labels.AddRange(
                codes[assignmentStart].labels);

            codes.RemoveRange(
                assignmentStart,
                addIndex - assignmentStart + 1);

            codes.InsertRange(assignmentStart, assignment);

            // Replace the vanilla Count check and First() selection

            List<CodeInstruction> selection = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldnull),
                CopyInstruction(storeEquipment)
            };

            selection[0].labels.AddRange(
                codes[selectionStart].labels);

            codes.RemoveRange(
                selectionStart,
                selectionEnd - selectionStart + 1);

            codes.InsertRange(selectionStart, selection);

            return codes;
        }

        private static List<int> FindCalls(
            List<CodeInstruction> codes,
            MethodInfo method)
        {
            List<int> result = new List<int>();

            if (method == null)
                return result;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(method))
                    result.Add(i);
            }

            return result;
        }

        private static CodeInstruction CopyInstruction(
            CodeInstruction instruction)
        {
            return new CodeInstruction(
                instruction.opcode,
                instruction.operand);
        }

        private static bool CanReplaceRange(
            List<CodeInstruction> codes,
            int start,
            int end)
        {
            HashSet<Label> internalLabels = new HashSet<Label>();

            for (int i = start; i <= end; i++)
            {
                if (codes[i].blocks.Count > 0)
                    return false;

                if (i != start)
                    internalLabels.UnionWith(codes[i].labels);
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (i >= start && i <= end)
                    continue;

                if (codes[i].operand is Label label &&
                    internalLabels.Contains(label))
                {
                    return false;
                }

                if (codes[i].operand is Label[] labels &&
                    labels.Any(internalLabels.Contains))
                {
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> Fail(
            List<CodeInstruction> codes,
            string reason)
        {
            Log.Error(
                "[UET] Failed to patch Ancient Mercenaries: " + reason);

            return codes;
        }
    }
}
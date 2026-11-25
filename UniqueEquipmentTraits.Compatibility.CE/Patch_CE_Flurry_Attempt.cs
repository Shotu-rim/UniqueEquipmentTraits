using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CombatExtended;
using HarmonyLib;
using UniqueEquipmentTraits.Patches;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    /// <summary>
    /// Connects Combat Extended melee attack attempts
    /// to the shared Flurry and Tailwind lifecycle.
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttackCE), "TryCastShot")]
    internal static class Patch_CE_Flurry_Attempt
    {
        [HarmonyPrefix]
        private static void Prefix(
            Verb_MeleeAttackCE __instance,
            out object __state)
        {
            Patch_Flurry_Attempt.BeginAttempt(
                __instance,
                out __state);
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo markResolvedMethod =
                AccessTools.Method(
                    typeof(Patch_Flurry_Attempt),
                    nameof(Patch_Flurry_Attempt.MarkResolved));

            if (markResolvedMethod == null)
            {
                Log.Error(
                    "[UET CE] Flurry: MarkResolved method not found.");

                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;

                yield break;
            }

            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!found &&
                    (instruction.opcode == OpCodes.Call ||
                     instruction.opcode == OpCodes.Callvirt) &&
                    instruction.operand is MethodInfo method &&
                    method.Name == "GetHitChance" &&
                    method.DeclaringType != null &&
                    typeof(Verb_MeleeAttackCE).IsAssignableFrom(
                        method.DeclaringType))
                {
                    found = true;

                    yield return instruction;

                    yield return new CodeInstruction(
                        OpCodes.Call,
                        markResolvedMethod);

                    continue;
                }

                yield return instruction;
            }

            if (!found)
            {
                Log.Error(
                    "[UET CE] Flurry: GetHitChance call not found. " +
                    "Charges will not be consumed.");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(object __state)
        {
            Patch_Flurry_Attempt.ConsumeAttempt(__state);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            object __state)
        {
            Patch_Flurry_Attempt.EndAttempt(__state);

            return __exception;
        }
    }
}
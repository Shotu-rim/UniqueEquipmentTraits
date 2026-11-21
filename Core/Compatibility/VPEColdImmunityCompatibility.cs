
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    public static class VPEColdImmunityCompatibility
    {
        public static void Apply(Harmony harmony)
        {
            PatchMethod(
                harmony,
                "VanillaPsycastsExpanded.IceBreatheProjectile",
                "DoDamage",
                new[] { typeof(IntVec3) });

            PatchMethod(
                harmony,
                "VanillaPsycastsExpanded.Hediff_BlizzardSource",
                "Tick",
                Type.EmptyTypes);

            PatchMethod(
                harmony,
                "VanillaPsycastsExpanded.Hediff_IceBlock",
                "Tick",
                Type.EmptyTypes);
        }

        private static void PatchMethod(
            Harmony harmony,
            string typeName,
            string methodName,
            Type[] parameters)
        {
            Type type = AccessTools.TypeByName(typeName);

            if (type == null)
                return;

            MethodInfo original = AccessTools.Method(
                type,
                methodName,
                parameters);

            MethodInfo transpiler = AccessTools.Method(
                typeof(VPEColdImmunityCompatibility),
                nameof(Transpiler));

            if (original == null || transpiler == null)
            {
                Log.Warning(
                    "[UET] VPE Cold Immunity patch skipped: " +
                    typeName + "." + methodName);
                return;
            }

            harmony.Patch(
                original,
                transpiler: new HarmonyMethod(transpiler));

            Log.Message(
                "[UET] VPE Cold Immunity patch installed: " +
                typeName + "." + methodName);
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            MethodInfo adjustSeverity = AccessTools.Method(
                typeof(HealthUtility),
                nameof(HealthUtility.AdjustSeverity),
                new[]
                {
                    typeof(Pawn),
                    typeof(HediffDef),
                    typeof(float)
                });

            MethodInfo replacement = AccessTools.Method(
                typeof(VPEColdImmunityCompatibility),
                nameof(AdjustColdSeverity));

            int replacements = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(adjustSeverity))
                {
                    CodeInstruction patched =
                        new CodeInstruction(instruction);

                    patched.opcode = OpCodes.Call;
                    patched.operand = replacement;

                    replacements++;
                    yield return patched;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (replacements == 0)
            {
                Log.Warning(
                    "[UET] VPE Cold Immunity: no AdjustSeverity " +
                    "calls found in " + original.DeclaringType.FullName +
                    "." + original.Name);
            }
        }

        public static void AdjustColdSeverity(
            Pawn pawn,
            HediffDef hediffDef,
            float amount)
        {
            if (pawn != null &&
                hediffDef != null &&
                amount > 0f &&
                (hediffDef == HediffDefOf.Hypothermia ||
                 hediffDef.defName == "VFEP_HypothermicSlowdown"))
            {
                Patch_HypothermiaImmunity.AdjustHypothermiaSeverity(
                    pawn,
                    hediffDef,
                    amount);

                return;
            }

            HealthUtility.AdjustSeverity(
                pawn,
                hediffDef,
                amount);
        }
    }
}

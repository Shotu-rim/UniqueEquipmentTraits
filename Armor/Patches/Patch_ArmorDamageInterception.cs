
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Shared integration point for armor traits
    /// that intercept incoming pawn damage.
    /// </summary>
    internal static class Patch_ArmorDamageInterception
    {

        private static void BeginDamageIteration(
            DamageWorker.DamageResult result)
        {
            ArmorDamageInterceptionContext context =
                ArmorDamageInterceptionScope.Current;

            if (context == null)
                return;

            context.BeginDamageIteration(
                result.totalDamageDealt);
        }



        private static float EndDamageIteration(
            DamageWorker.DamageResult result)
        {
            ArmorDamageInterceptionContext context =
                ArmorDamageInterceptionScope.Current;

            if (context == null)
                return result.totalDamageDealt;

            // Vanilla uses cumulative totalDamageDealt. Save it and include all damage intercepted by armor during ApplyAllDamage
            return result.totalDamageDealt +
                   context.AbsorbedDamage;
        }

        private static bool TryInterceptDamage(
            Pawn pawn,
            float damage)
        {
            if (pawn == null || damage <= 0f)
                return false;

            return ArmorPassiveDamageUtility
                .TryInterceptAndRecord(pawn, damage);
        }




        private static bool InterceptPartDamage(
            DamageInfo dinfo,
            Pawn pawn,
            DamageWorker.DamageResult result,
            float damage)
        {
            if (!TryInterceptDamage(pawn, damage))
                return false;

            result.AddPart(pawn, dinfo.HitPart);
            result.deflected = true;

            ArmorReactionAttackObserver.ObserveAbsorbedHit(
                pawn,
                dinfo);

            return true;
        }


        [HarmonyPatch]
        private static class ApplyDamageToPartTranspiler
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "ApplyDamageToPart",
                    new Type[]
                    {
                typeof(DamageInfo),
                typeof(Pawn),
                typeof(DamageWorker.DamageResult)
                    });
            }

            private static void Prefix()
            {
                ArmorDamageInterceptionContext context =
                    ArmorDamageInterceptionScope.Current;

                if (context != null)
                    context.BeginPartDamage();
            }

            private static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions,
                ILGenerator generator)
            {
                List<CodeInstruction> codes =
                    new List<CodeInstruction>(instructions);

                MethodInfo isHeadshot = AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "IsHeadshot");

                MethodInfo postArmorDamage = AccessTools.Method(
                    typeof(ArmorUtility),
                    "GetPostArmorDamage");

                MethodInfo intercept = AccessTools.Method(
                    typeof(Patch_ArmorDamageInterception),
                    "InterceptPartDamage");

                if (isHeadshot == null ||
                    postArmorDamage == null ||
                    intercept == null)
                {

                    return codes;
                }

                int headshotIndex = -1;
                int headshotMatches = 0;

                for (int i = 2; i < codes.Count; i++)
                {
                    if (!codes[i].Calls(isHeadshot))
                        continue;

                    if (codes[i - 2].opcode != OpCodes.Ldarg_1 ||
                        codes[i - 1].opcode != OpCodes.Ldarg_2)
                        continue;

                    headshotIndex = i - 2;
                    headshotMatches++;
                }

                if (headshotMatches != 1)
                {
                    Log.Error(
                        "[UET Aegis] ApplyDamageToPart: " +
                        "IsHeadshot pattern not unique. Found " +
                        headshotMatches + ".");

                    return codes;
                }

                // Post-armor damage result is stored after GetPostArmorDamage()
                int armorStoreIndex = -1;
                int armorStoreMatches = 0;

                for (int i = 0; i < headshotIndex - 1; i++)
                {
                    if (!codes[i].Calls(postArmorDamage))
                        continue;

                    if (!IsStoreLocal(codes[i + 1]))
                        continue;

                    armorStoreIndex = i + 1;
                    armorStoreMatches++;
                }

                if (armorStoreMatches != 1)
                {
                    Log.Error(
                        "[UET Aegis] ApplyDamageToPart: " +
                        "post-armor damage local not resolved. Found " +
                        armorStoreMatches + ".");

                    return codes;
                }

                CodeInstruction damageLoad =
                    GetLoadForStore(codes[armorStoreIndex]);

                if (damageLoad == null)
                {
                    Log.Error(
                        "[UET Aegis] ApplyDamageToPart: " +
                        "unsupported damage local.");

                    return codes;
                }

                // Add interception before IsHeadshot call
                Label continueLabel = generator.DefineLabel();
                Label returnLabel = generator.DefineLabel();

                List<CodeInstruction> injection =
                    new List<CodeInstruction>
                    {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_3),
                damageLoad,
                new CodeInstruction(OpCodes.Call, intercept),
                new CodeInstruction(OpCodes.Brfalse, continueLabel),
                new CodeInstruction(OpCodes.Br, returnLabel)
                    };

                // Transfer IsHeadshot's to the injection
                injection[0].labels.AddRange(
                    codes[headshotIndex].labels);

                codes[headshotIndex].labels.Clear();
                codes[headshotIndex].labels.Add(continueLabel);

                codes.InsertRange(headshotIndex, injection);

                CodeInstruction ret =
                    new CodeInstruction(OpCodes.Ret);


                ret.labels.Add(returnLabel);
                codes.Add(ret);

                // Observe vanilla hits fully absorbed by armor.
                // Locate the return immediately following
                // DamageResult.deflectedByMetalArmor assignment.

                FieldInfo deflectedByMetalArmorField =
                    AccessTools.Field(
                        typeof(DamageWorker.DamageResult),
                        "deflectedByMetalArmor");

                MethodInfo observeAbsorbedHit =
                    AccessTools.Method(
                        typeof(ArmorReactionAttackObserver),
                        "ObserveAbsorbedHit");

                if (deflectedByMetalArmorField == null ||
                    observeAbsorbedHit == null)
                {
                    Log.Error(
                        "[UET Reactive Armor] Required member " +
                        "not found. Vanilla absorption observer " +
                        "not installed.");

                    return codes;
                }

                int vanillaReturnIndex = -1;
                int vanillaReturnMatches = 0;

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode != OpCodes.Stfld ||
                        !Equals(
                            codes[i].operand,
                            deflectedByMetalArmorField))
                    {
                        continue;
                    }

                    if (codes[i + 1].opcode != OpCodes.Ret)
                        continue;

                    vanillaReturnIndex = i + 1;
                    vanillaReturnMatches++;
                }

                if (vanillaReturnMatches != 1)
                {
                    Log.Error(
                        "[UET Reactive Armor] Vanilla absorption " +
                        "return pattern not unique. Found " +
                        vanillaReturnMatches + ".");

                    return codes;
                }

                // Do not alter an existing branch target.
                if (codes[vanillaReturnIndex].labels.Count > 0 ||
                    codes[vanillaReturnIndex].blocks.Count > 0)
                {
                    Log.Error(
                        "[UET Reactive Armor] Vanilla absorption " +
                        "return has labels or exception blocks. " +
                        "Observer not installed.");

                    return codes;
                }

                codes.InsertRange(
                    vanillaReturnIndex,
                    new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(
                        OpCodes.Call,
                        observeAbsorbedHit)
                    });

                return codes;

            }

            private static bool IsStoreLocal(CodeInstruction instruction)
            {
                return instruction.opcode == OpCodes.Stloc_0 ||
                       instruction.opcode == OpCodes.Stloc_1 ||
                       instruction.opcode == OpCodes.Stloc_2 ||
                       instruction.opcode == OpCodes.Stloc_3 ||
                       instruction.opcode == OpCodes.Stloc ||
                       instruction.opcode == OpCodes.Stloc_S;
            }

            private static CodeInstruction GetLoadForStore(
                CodeInstruction store)
            {
                if (store.opcode == OpCodes.Stloc_0)
                    return new CodeInstruction(OpCodes.Ldloc_0);

                if (store.opcode == OpCodes.Stloc_1)
                    return new CodeInstruction(OpCodes.Ldloc_1);

                if (store.opcode == OpCodes.Stloc_2)
                    return new CodeInstruction(OpCodes.Ldloc_2);

                if (store.opcode == OpCodes.Stloc_3)
                    return new CodeInstruction(OpCodes.Ldloc_3);

                if (store.opcode == OpCodes.Stloc ||
                    store.opcode == OpCodes.Stloc_S)
                {
                    return new CodeInstruction(
                        store.opcode == OpCodes.Stloc
                            ? OpCodes.Ldloc
                            : OpCodes.Ldloc_S,
                        store.operand);
                }

                return null;
            }
        }



        [HarmonyPatch]
        private static class SmallPawnDamagePropagationPatch
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "ApplySmallPawnDamagePropagation");
            }


            private static bool Prefix()
            {
                ArmorDamageInterceptionContext context =
                    ArmorDamageInterceptionScope.Current;

                if (context == null)
                    return true;

                return !context.LastPartIntercepted;
            }

        }


        [HarmonyPatch]
        private static class ApplyAllDamageTranspiler
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "ApplyToPawn",
                    new Type[]
                    {
                typeof(DamageInfo),
                typeof(Pawn)
                    });
            }


            private static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes =
                    new List<CodeInstruction>(instructions);

                MethodInfo applyDamageToPart = AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "ApplyDamageToPart");

                MethodInfo beginIteration = AccessTools.Method(
                    typeof(Patch_ArmorDamageInterception),
                    "BeginDamageIteration");

                MethodInfo endIteration = AccessTools.Method(
                    typeof(Patch_ArmorDamageInterception),
                    "EndDamageIteration");

                FieldInfo totalDamageField = AccessTools.Field(
                    typeof(DamageWorker.DamageResult),
                    "totalDamageDealt");

                if (applyDamageToPart == null ||
                    beginIteration == null ||
                    endIteration == null ||
                    totalDamageField == null)
                {
                    Log.Error(
                        "[UET Aegis] ApplyAllDamage: " +
                        "required member not found.");

                    return codes;
                }

                int matchCount = 0;
                int callIndex = -1;
                int fieldIndex = -1;

                for (int i = 1; i < codes.Count - 3; i++)
                {
                    if (!codes[i].Calls(applyDamageToPart))
                        continue;

                    // ApplyDamageToPart must receive DamageResult
                    CodeInstruction resultLoad = codes[i - 1];

                    bool validResultLoad =
                        resultLoad.opcode == OpCodes.Ldloc_0 ||
                        resultLoad.opcode == OpCodes.Ldloc_1 ||
                        resultLoad.opcode == OpCodes.Ldloc_2 ||
                        resultLoad.opcode == OpCodes.Ldloc_3 ||
                        resultLoad.opcode == OpCodes.Ldloc ||
                        resultLoad.opcode == OpCodes.Ldloc_S;

                    if (!validResultLoad)
                        continue;

                    // Find: ldloc damageResult, ldfld totalDamageDealt, sub
                    int searchEnd = Math.Min(
                        codes.Count - 2,
                        i + 20);

                    for (int j = i + 2; j < searchEnd; j++)
                    {
                        if (codes[j].opcode != OpCodes.Ldfld ||
                            !Equals(codes[j].operand, totalDamageField))
                            continue;

                        if (codes[j + 1].opcode != OpCodes.Sub)
                            continue;

                        matchCount++;
                        callIndex = i;
                        fieldIndex = j;
                    }
                }

                if (matchCount != 1)
                {
                    Log.Error(
                        "[UET Aegis] ApplyAllDamage: " +
                        "expected one damage loop, found " +
                        matchCount + ". Patch not applied.");

                    return codes;
                }

                // Consumes DamageResult and uses cumulative damage value. Replace it with DamageResult and return current damage

                CodeInstruction originalField = codes[fieldIndex];

                CodeInstruction replacement =
                    new CodeInstruction(
                        OpCodes.Call,
                        endIteration);

                replacement.labels.AddRange(
                    originalField.labels);

                replacement.blocks.AddRange(
                    originalField.blocks);

                codes[fieldIndex] = replacement;

                // Evaluation stack contains: this, DamageInfo, Pawn, DamageResult. Duplicate DamageResult, then save original ApplyDamageToPart

                codes.Insert(
                    callIndex,
                    new CodeInstruction(OpCodes.Dup));

                codes.Insert(
                    callIndex + 1,
                    new CodeInstruction(
                        OpCodes.Call,
                        beginIteration));

                return codes;
            }

        }

        [HarmonyPatch]
        private static class ApplyToPawnScope
        {

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(
                    typeof(DamageWorker_AddInjury),
                    "ApplyToPawn",
                    new Type[]
                    {
                        typeof(DamageInfo),
                        typeof(Pawn)
                    });
            }

            private static void Prefix()
            {
                ArmorDamageInterceptionScope.Begin();

            }

            private static Exception Finalizer(
                Exception __exception)
            {
                ArmorDamageInterceptionScope.End();
                return __exception;
            }
        }
    }
}

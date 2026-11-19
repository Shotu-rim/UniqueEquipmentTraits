
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Patches
{
    /// <summary>
    /// Patches diseases from injuries
    /// </summary>
    [HarmonyPatch(typeof(HediffComp_Infecter), "CheckMakeInfection")]
    internal static class Patch_HediffComp_Infecter_CheckMakeInfection
    {
        private static float ApplyResilient(
            float chance,
            HediffComp_Infecter infecter)
        {
            if (infecter == null || infecter.Pawn == null)
                return chance;

            float immunityGainBonus;
            float diseaseRiskReduction;

            ArmorTraitResilientUtility.GetBonuses(
                infecter.Pawn,
                out immunityGainBonus,
                out diseaseRiskReduction);

            return Mathf.Clamp01(
                chance * (1f - diseaseRiskReduction));
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(instructions);

            MethodInfo randomValueGetter =
                AccessTools.PropertyGetter(
                    typeof(Rand),
                    nameof(Rand.Value));

            MethodInfo applyResilient =
                AccessTools.Method(
                    typeof(Patch_HediffComp_Infecter_CheckMakeInfection),
                    nameof(ApplyResilient));

            int matches = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(randomValueGetter))
                    continue;

                // Next instruction loads infection chance. Modify that chance before comparison.
                int chanceIndex = i + 1;

                if (chanceIndex >= codes.Count)
                    continue;

                if (!codes[chanceIndex].IsLdloc())
                    continue;

                codes.Insert(
                    chanceIndex + 1,
                    new CodeInstruction(OpCodes.Ldarg_0));

                codes.Insert(
                    chanceIndex + 2,
                    new CodeInstruction(
                        OpCodes.Call,
                        applyResilient));

                matches++;
                break;
            }

            if (matches != 1)
            {
                Log.Error(
                    "[UET Resilient] Could not locate " +
                    "the wound infection chance roll.");
            }

            return codes;
        }
    }
}

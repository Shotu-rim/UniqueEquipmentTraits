
using HarmonyLib;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    [HarmonyPatch(
        typeof(StunHandler),
        "CanAdaptToDamage")]
    public static class Patch_StunningPulseAdaptation
    {
        [HarmonyPrefix]
        public static bool Prefix(
            DamageDef def,
            ref bool __result)
        {
            if (def != null &&
                def.defName == "UET_StunningPulse")
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}

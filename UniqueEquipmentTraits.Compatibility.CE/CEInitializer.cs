using HarmonyLib;
using Verse;

namespace UniqueEquipmentTraits.Compatibility.CE
{
    [StaticConstructorOnStartup]
    public static class CEInitializer
    {
        static CEInitializer()
        {
            var harmony = new Harmony(
                "shotu.uniqueequipmenttraits.ce");

            harmony.PatchAll(
                typeof(CEInitializer).Assembly);

            Log.Message(
                "[UET CE] Combat Extended Compatibility module initialized.");
        }
    }
}
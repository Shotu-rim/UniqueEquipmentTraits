
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitTestAction
    {
        [DebugAction(
            "UET",
            "Test Armor Traits on all worn apparel",
            actionType = DebugActionType.Action)]
        private static void TestArmorTraits()
        {
            Map map = Find.CurrentMap;

            if (map == null)
            {
                Log.Warning("[UET ARMOR TEST] No active map.");
                return;
            }

            ArmorTraitDef testTrait =
                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                    "UET_TestReactive");

            if (testTrait == null)
            {
                Log.Error("[UET ARMOR TEST] Test trait not found.");
                return;
            }

            Pawn selectedPawn = null;

            foreach (object selected in Find.Selector.SelectedObjects)
            {
                selectedPawn = selected as Pawn;

                if (selectedPawn != null)
                    break;
            }

            if (selectedPawn == null)
            {
                Log.Warning(
                    "[UET ARMOR TEST] Select a pawn first.");
                return;
            }

            if (selectedPawn.apparel == null)
            {
                Log.Warning(
                    "[UET ARMOR TEST] Selected pawn has no apparel tracker.");
                return;
            }

            int eligible = 0;
            int added = 0;
            int alreadyPresent = 0;

            foreach (Apparel apparel in selectedPawn.apparel.WornApparel)
            {
                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null)
                    continue;

                eligible++;

                bool wasAdded = comp.AddTrait(testTrait);

                if (wasAdded)
                    added++;
                else
                    alreadyPresent++;

                Log.Message(
                    "[UET ARMOR TEST] " +
                    apparel.def.defName +
                    " | Added: " + wasAdded +
                    " | Traits: " + comp.Traits.Count +
                    " | Reactive: " + comp.HasReactiveTrait);
            }

            Log.Message(
                "[UET ARMOR TEST] Pawn: " +
                selectedPawn.LabelShort +
                " | Eligible apparel: " + eligible +
                " | Added: " + added +
                " | Already present: " + alreadyPresent);
        }
    }
}

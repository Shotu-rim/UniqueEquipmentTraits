
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    public static class UETDevForceConversion
    {
        [DebugAction(
            "UET",
            "Force convert equipment",
            actionType = DebugActionType.ToolMap)]
        private static void ForceConvert()
        {
            IntVec3 cell = UI.MouseCell();
            Map map = Find.CurrentMap;

            if (map == null || !cell.InBounds(map))
                return;

            Thing original = null;

            foreach (Thing thing in cell.GetThingList(map))
            {
                if (UETWorldConversionEligibility.IsEligible(thing))
                {
                    original = thing;
                    break;
                }
            }

            if (original == null)
            {
                Messages.Message(
                    "UET: No eligible equipment in selected cell.",
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Thing converted;

            if (!UETWorldConversionThingFactory.TryCreate(
                original, out converted))
            {
                Messages.Message(
                    "UET: Conversion failed. Check the log.",
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            IntVec3 position = original.Position;
            Rot4 rotation = original.Rotation;

            // Najpierw usuń oryginał z mapy.
            original.DeSpawn();

            Thing spawned = GenSpawn.Spawn(
                converted,
                position,
                map,
                rotation);

            if (spawned == null)
            {
                // Przywróć oryginał, jeśli podmiana się nie uda.
                GenSpawn.Spawn(original, position, map, rotation);
                converted.Destroy();
                return;
            }

            original.Destroy();

            CompQuality quality =
                converted.TryGetComp<CompQuality>();

            Log.Message(
                "[UET DEV CONVERSION] " +
                original.def.defName +
                " -> " + converted.def.defName +
                " | Quality: " +
                (quality != null
                    ? quality.Quality.ToString()
                    : "NONE"));

            Messages.Message(
                "UET: Equipment converted successfully.",
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}

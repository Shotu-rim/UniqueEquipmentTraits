
using LudeonTK;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Utilities
{
    /// <summary>
    /// Provides a developer map tool for manually converting eligible
    /// equipment into UET unique variants.
    /// Replaces the selected item while preserving its position and rotation,
    /// and attempts to restore the original if spawning fails.
    /// Intended for testing world conversion behavior and compatibility.
    /// </summary>
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

            // Remove the original item from the map before replacement
            original.DeSpawn();

            Thing spawned = GenSpawn.Spawn(
                converted,
                position,
                map,
                rotation);

            if (spawned == null)
            {
                // Restore the original item on fails
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

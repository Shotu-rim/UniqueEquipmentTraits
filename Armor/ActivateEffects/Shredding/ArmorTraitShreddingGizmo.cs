
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitShreddingGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null)
                return null;

            Apparel apparel = comp.parent as Apparel;
            Pawn pawn = apparel != null ? apparel.Wearer : null;

            ArmorTraitState state = comp.State;

            Command_Action command = new Command_Action
            {
                defaultLabel = "Shredding",
                defaultDesc =
                    "Amplifies bleeding from the target's " +
                    "currently bleeding injuries.\n\n" +
                    "Masterwork: x1.75 bleeding.\n" +
                    "Legendary: x2.25 bleeding.\n\n" +
                    "Repeated applications stack multiplicatively.",
                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Shredding",
                    false)
            };

            command.action = delegate
            {
                if (pawn == null || state == null)
                    return;

                float range =
                    ArmorTraitMedicUtility.GetRange(quality);

                TargetingParameters targeting =
                    new TargetingParameters
                    {
                        canTargetPawns = true,
                        canTargetBuildings = false,
                        canTargetItems = false,
                        canTargetLocations = false,

                        validator = target =>
                        {
                            Pawn targetPawn =
                                target.Thing as Pawn;

                            return ArmorTraitShreddingUtility.CanApply(
                                pawn,
                                targetPawn,
                                quality,
                                state);
                        }
                    };

                Find.Targeter.BeginTargeting(
                    targeting,

                    // Activation
                    delegate (LocalTargetInfo target)
                    {
                        Pawn targetPawn =
                            target.Thing as Pawn;

                        if (targetPawn == null)
                            return;

                        ArmorTraitShreddingUtility.TryApply(
                            pawn,
                            targetPawn,
                            quality,
                            state);
                    },

                    // Range indicator
                    delegate (LocalTargetInfo target)
                    {
                        if (pawn != null && pawn.Spawned)
                        {
                            GenDraw.DrawRadiusRing(
                                pawn.Position,
                                range);
                        }
                    },

                    // Target validation
                    delegate (LocalTargetInfo target)
                    {
                        Pawn targetPawn =
                            target.Thing as Pawn;

                        return targetPawn != null &&
                            ArmorTraitShreddingUtility.CanApply(
                                pawn,
                                targetPawn,
                                quality,
                                state);
                    },

                    caster: pawn
                );
            };

            // Wearer validation
            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                !pawn.Spawned)
            {
                command.Disable(
                    "The wearer cannot use Shredding.");

                return command;
            }

            // Charge validation
            if (state == null ||
                !state.HasAvailableCharge)
            {
                command.Disable(
                    "No Shredding charges available.");

                return command;
            }

            // Cooldown validation
            if (state.IsOnCooldown)
            {
                command.Disable(
                    "Shredding is on cooldown.");

                return command;
            }

            return command;
        }
    }
}

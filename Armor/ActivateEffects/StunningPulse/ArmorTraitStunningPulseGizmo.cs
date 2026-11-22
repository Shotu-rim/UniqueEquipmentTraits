
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitStunningPulseGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null || comp.State == null)
                return null;

            ArmorTraitState state = comp.State;

            Apparel apparel = comp.parent as Apparel;

            Pawn pawn = apparel != null
                ? apparel.Wearer
                : null;

            float range =
                ArmorTraitStunningPulseUtility.GetRange();

            int stunTicks =
                ArmorTraitStunningPulseUtility.GetStunTicks(
                    quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Stunning Pulse",

                defaultDesc =
                    "Releases a focused pulse that stuns a selected pawn." +
                    "\n\nStun duration: " +
                    (stunTicks / 60f).ToString("0.#") + " seconds" +
                    "\nRange: " +
                    range.ToString("0") + " cells" +
                    "\nCharges: " +
                    state.Charges + "/" + state.MaxCharges +
                    "\nCharge regeneration: " +
                    (ArmorTraitState.GetChargeRegenTicks(quality) / 60000f)
                    .ToString("0.#") + " days" +
                    "\nCooldown: 10 seconds" +
                    "\nAdaptation: 20 seconds",

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Abilities/Stun",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (pawn == null ||
                        pawn.Map == null ||
                        state.IsOnCooldown ||
                        !state.HasAvailableCharge)
                        return;

                    TargetingParameters targeting =
                        new TargetingParameters
                        {
                            canTargetLocations = false,
                            canTargetPawns = true,
                            canTargetBuildings = false,
                            canTargetItems = false
                        };

                    Find.Targeter.BeginTargeting(
                        targeting,

                        // Activation
                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn =
                                target.Thing as Pawn;

                            if (targetPawn == null ||
                                apparel.Wearer != pawn)
                                return;


                            if (!ArmorTraitStunningPulseUtility.CanActivate(
                                pawn,
                                targetPawn,
                                quality,
                                state))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_StunningPulse");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_StunningPulse");
                                return;
                            }

                            ArmorTraitCastUtility.TryStartTargetedCast(
                                pawn,
                                apparel,
                                trait,
                                targetPawn);

                        },

                        // Range indicator
                        delegate (LocalTargetInfo target)
                        {
                            if (pawn != null && pawn.Spawned)
                            {
                                GenDraw.DrawRadiusRing(
                                    pawn.Position,
                                    range);
                                ArmorTraitTargetingUtility.DrawHoveredTarget();
                            }
                        },

                        // Target validation
                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn =
                                target.Thing as Pawn;

                            return targetPawn != null &&
                                ArmorTraitStunningPulseUtility.CanActivate(
                                    pawn,
                                    targetPawn,
                                    quality,
                                    state);
                        },

                        caster: pawn
                    );
                }
            };

            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                !pawn.Spawned)
            {
                command.Disable(
                    "Wearer cannot activate Stunning Pulse.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Stunning Pulse is on cooldown.");
            }

            return command;
        }
    }
}

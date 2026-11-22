
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitOppressionGizmo
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
                ArmorTraitOppressionUtility.GetRange(
                    quality);

            int durationTicks =
                ArmorTraitOppressionUtility.GetDurationTicks(
                    quality);

            float baseSlowdown =
                ArmorTraitOppressionUtility.GetBaseSlowdown(
                    quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Oppression",

                defaultDesc =
                    "Inflicts an oppressive force on a selected pawn, " +
                    "reducing its movement and attack speed." +
                    "\n\nBase slowdown: " +
                    (baseSlowdown * 100f).ToString("0.#") + "%" +
                    "\nDuration: " +
                    (durationTicks / 60f).ToString("0.#") +
                    " seconds" +
                    "\nRange: " +
                    range.ToString("0") + " cells" +
                    "\nCharges: " +
                    state.Charges + "/" + state.MaxCharges +
                    "\nCharge regeneration: " +
                    (ArmorTraitState.GetChargeRegenTicks(
                        quality) / 60000f).ToString("0.#") +
                    " days" +
                    "\nCooldown: " +
                    (ArmorTraitOppressionUtility.GetCooldownTicks()
                        / 60f).ToString("0.#") +
                    " seconds" +
                    "\n\nEffectiveness depends on the target's " +
                    "body size, body mass and equipped gear.",

                // Temporary icon until we create
                // the dedicated Oppression artwork.
                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Oppression",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (pawn == null ||
                        pawn.Map == null ||
                        state.IsOnCooldown ||
                        !state.HasAvailableCharge)
                    {
                        return;
                    }

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
                            {
                                return;
                            }


                            if (!ArmorTraitOppressionUtility.CanActivate(
                                pawn,
                                targetPawn,
                                quality,
                                state))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_Oppression");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_Oppression");
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
                                ArmorTraitOppressionUtility.CanActivate(
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
                    "Wearer cannot activate Oppression.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Oppression is on cooldown.");
            }

            return command;
        }
    }
}

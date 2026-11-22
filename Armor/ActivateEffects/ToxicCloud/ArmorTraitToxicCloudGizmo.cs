
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitToxicCloudGizmo
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
                ArmorTraitToxicCloudUtility.GetRange(quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Toxic Cloud",
                defaultDesc =
                    "Releases a cloud of toxic gas at a targeted location." +
                    "\n\nRange: " + range.ToString("0") +
                    " cells" +
                    "\nCooldown between uses: 5 seconds" +
                    "\nGoodwill penalty: " +
                    ArmorTraitToxicCloudUtility
                        .GetGoodwillPenalty(quality),

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_ToxicCloud",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (pawn == null ||
                        pawn.Map == null ||
                        !state.HasAvailableCharge ||
                        state.IsOnCooldown)
                    {
                        return;
                    }

                    TargetingParameters targeting =
                        new TargetingParameters
                        {
                            canTargetLocations = true,
                            canTargetPawns = true,
                            canTargetBuildings = true,
                            canTargetItems = true
                        };

                    Find.Targeter.BeginTargeting(
                        targeting,

                        // Activation
                        delegate (LocalTargetInfo target)
                        {
                            if (!target.IsValid)
                                return;


                            if (apparel == null ||
                                apparel.Wearer != pawn)
                            {
                                return;
                            }

                            if (!ArmorTraitToxicCloudUtility.CanRelease(
                                pawn,
                                target.Cell,
                                quality,
                                state))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_ToxicCloud");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_ToxicCloud");
                                return;
                            }

                            ArmorTraitCastUtility.TryStartTargetedCast(
                                pawn,
                                apparel,
                                trait,
                                target.Cell);

                        },

                        // Ring limits
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

                        // Cell validation
                        delegate (LocalTargetInfo target)
                        {
                            return target.IsValid &&
                                ArmorTraitToxicCloudUtility.CanRelease(
                                    pawn,
                                    target.Cell,
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
                    "Wearer cannot activate Toxic Cloud.");
            }
            else if (!ModsConfig.BiotechActive)
            {
                command.Disable(
                    "Toxic Cloud requires Biotech.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Cooldown: " +
                    (state.RemainingCooldownTicks / 60f)
                        .ToString("0.#") +
                    " seconds remaining.");
            }

            return command;
        }
    }
}

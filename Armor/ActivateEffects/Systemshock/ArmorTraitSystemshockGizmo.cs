
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSystemshockGizmo
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
                ArmorTraitSystemshockUtility.GetRange(quality);

            float radius =
                ArmorTraitSystemshockUtility.GetRadius(quality);

            int cooldownTicks =
                ArmorTraitSystemshockUtility.GetCooldownTicks(quality);

            int goodwillPenalty =
                ArmorTraitSystemshockUtility.GetGoodwillPenalty(quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "EMP Burst",

                defaultDesc =
                    "Releases an instantaneous electromagnetic " +
                    "pulse at a selected location, disrupting " +
                    "susceptible targets within its radius." +
                    "\n\nRadius: " +
                    radius.ToString("0") + " cells" +
                    "\nRange: " +
                    range.ToString("0") + " cells" +
                    "\nCharges: " +
                    state.Charges + "/" + state.MaxCharges +
                    "\nCharge regeneration: " +
                    (ArmorTraitState.GetChargeRegenTicks(
                        quality) / 60000f).ToString("0.#") +
                    " days" +
                    "\nCooldown: " +
                    (cooldownTicks / 60f).ToString("0.#") +
                    " seconds" +
                    "\nGoodwill penalty: " +
                    goodwillPenalty +
                    " per affected neutral or allied faction" +
                    "\n\nAffects friendly units and the wearer. " +
                    "Walls block the pulse. Goodwill is lost " +
                    "when neutral or allied mechanoids or pawns " +
                    "with bionic or prosthetic implants are hit, " +
                    "even if the EMP causes no actual effect.",

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Systemshock",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (apparel == null ||
                        apparel.Wearer != pawn ||
                        pawn == null ||
                        pawn.Map == null ||
                        state.IsOnCooldown ||
                        state.IsEffectActive ||
                        !state.HasAvailableCharge)
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

                        // Activation (to be implemented)
                        delegate (LocalTargetInfo target)
                        {
                            if (apparel.Wearer != pawn ||
                                !ArmorTraitSystemshockUtility.CanActivate(
                                    pawn,
                                    target.Cell,
                                    quality,
                                    state))
                            {
                                return;
                            }



                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_Systemshock");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_Systemshock");
                                return;
                            }

                            ArmorTraitCastUtility.TryStartTargetedCast(
                                pawn,
                                apparel,
                                trait,
                                target.Cell);


                        },

                        // Range and blast radius indicators
                        delegate (LocalTargetInfo target)
                        {
                            if (pawn != null && pawn.Spawned)
                            {
                                GenDraw.DrawRadiusRing(
                                    pawn.Position,
                                    range);
                            }

                            if (target.IsValid &&
                                pawn != null &&
                                pawn.Map != null &&
                                target.Cell.InBounds(pawn.Map))
                            {
                                GenDraw.DrawRadiusRing(
                                    target.Cell,
                                    radius);
                            }
                        },

                        // Target validation
                        delegate (LocalTargetInfo target)
                        {
                            return ArmorTraitSystemshockUtility.CanActivate(
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
                    "Wearer cannot activate EMP Burst.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "EMP Burst is on cooldown.");
            }
            else if (state.IsEffectActive)
            {
                command.Disable(
                    "EMP Burst is already active.");
            }

            return command;
        }
    }
}

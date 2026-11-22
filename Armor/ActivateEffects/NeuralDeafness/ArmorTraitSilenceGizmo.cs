
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSilenceGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null ||
                comp.State == null ||
                quality != QualityCategory.Legendary)
            {
                return null;
            }

            ArmorTraitState state = comp.State;
            Apparel apparel = comp.parent as Apparel;

            Pawn caster = apparel != null
                ? apparel.Wearer
                : null;

            float range =
                ArmorTraitSilenceUtility.GetRange();

            Command_Action command = new Command_Action
            {
                defaultLabel = "Silence",

                defaultDesc =
                    "Temporarily suppresses the target's " +
                    "psychic sensitivity.\n\n" +
                    "Range: " + range.ToString("0") +
                    " cells\n" +
                    "Duration: 2 hours\n" +
                    "Charges: " +
                    state.Charges + "/" + state.MaxCharges,

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Silence",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (caster == null ||
                        caster.Dead ||
                        caster.Downed ||
                        !caster.Spawned ||
                        caster.Map == null ||
                        apparel.Wearer != caster ||
                        !state.HasAvailableCharge)
                    {
                        return;
                    }

                    TargetingParameters targeting =
                        new TargetingParameters
                        {
                            canTargetPawns = true,
                            canTargetBuildings = false,
                            canTargetItems = false,
                            canTargetLocations = false
                        };

                    Find.Targeter.BeginTargeting(
                        targeting,

                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn =
                                target.Thing as Pawn;

                            if (!ArmorTraitSilenceUtility.CanActivate(
                                caster,
                                targetPawn,
                                quality,
                                state))
                            {
                                return;
                            }

                            if (apparel.Wearer != caster)
                                return;

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>
                                    .GetNamedSilentFail(
                                        "UET_NeuralDeafness");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: " +
                                    "UET_NeuralDeafness");
                                return;
                            }

                            ArmorTraitCastUtility.TryStartTargetedCast(
                                caster,
                                apparel,
                                trait,
                                targetPawn);
                        },

                        delegate (LocalTargetInfo target)
                        {
                            if (caster != null && caster.Spawned)
                            {
                                GenDraw.DrawRadiusRing(
                                    caster.Position,
                                    range);
                            }
                        },

                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn =
                                target.Thing as Pawn;

                            return apparel.Wearer == caster &&
                                ArmorTraitSilenceUtility.CanActivate(
                                    caster,
                                    targetPawn,
                                    quality,
                                    state);
                        },

                        caster: caster);
                }
            };

            if (caster == null ||
                caster.Dead ||
                caster.Downed ||
                !caster.Spawned)
            {
                command.Disable(
                    "Wearer cannot activate Silence.");
            }
            else if (state.MaxCharges <= 0)
            {
                command.Disable("No charges configured.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable("No charges available.");
            }

            return command;
        }
    }
}

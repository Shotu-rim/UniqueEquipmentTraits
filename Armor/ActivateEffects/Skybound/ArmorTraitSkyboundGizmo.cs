
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitSkyboundGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null || comp.State == null)
                return null;

            ArmorTraitState state = comp.State;

            float range =
                ArmorTraitSkyboundUtility.GetJumpRange(quality);

            int cooldownTicks =
                ArmorTraitSkyboundUtility.GetCooldownTicks(quality);

            Texture2D jumpIcon =
                ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Skybound",
                    true);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Jump",
                icon = jumpIcon,

                defaultDesc =
                    "Jump to a selected location.\n\n" +
                    "Range: " + range.ToString("0.#") + " cells\n" +
                    "Cooldown: " +
                    (cooldownTicks / 60f).ToString("0.#") +
                    " seconds",

                action = delegate
                {
                    Apparel apparel = comp.parent as Apparel;

                    Pawn caster =
                        apparel != null ? apparel.Wearer : null;

                    if (caster == null ||
                        caster.Dead ||
                        caster.Map == null ||
                        state.IsOnCooldown)
                    {
                        return;
                    }

                    TargetingParameters targetParams =
                        new TargetingParameters
                        {
                            canTargetLocations = true,
                            canTargetPawns = false,
                            canTargetBuildings = false,
                            canTargetItems = false
                        };

                    Find.Targeter.BeginTargeting(
                        targetParams,
                        delegate (LocalTargetInfo target)
                        {
                            if (apparel.Wearer != caster ||
                                caster.Dead ||
                                !caster.Spawned)
                            {
                                return;
                            }


                            if (!target.IsValid ||
                                !JumpUtility.ValidJumpTarget(
                                    caster,
                                    caster.Map,
                                    target.Cell) ||
                                !JumpUtility.CanHitTargetFrom(
                                    caster,
                                    caster.Position,
                                    target,
                                    range))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_Skybound");

                            if (trait == null)
                            {
                                Log.Error("[UET] Missing ArmorTraitDef: UET_Skybound");
                                return;
                            }

                            ArmorTraitCastUtility.TryStartTargetedCast(
                                caster,
                                apparel,
                                trait,
                                target.Cell);

                        },
                        delegate
                        {
                            if (caster.Spawned &&
                                apparel.Wearer == caster)
                            {
                                GenDraw.DrawRadiusRing(
                                    caster.Position,
                                    range);
                                ArmorTraitTargetingUtility.DrawHoveredTarget();
                            }
                        },
                        delegate (LocalTargetInfo target)
                        {
                            return caster.Spawned &&
                                   apparel.Wearer == caster &&
                                   !state.IsOnCooldown &&
                                   JumpUtility.ValidJumpTarget(
                                       caster,
                                       caster.Map,
                                       target.Cell) &&
                                   JumpUtility.CanHitTargetFrom(
                                       caster,
                                       caster.Position,
                                       target,
                                       range);
                        },
                        caster);
                }
            };

            if (state.IsOnCooldown)
            {
                float remaining =
                    state.RemainingCooldownTicks / 60f;

                command.Disable(
                    "Cooldown: " +
                    remaining.ToString("0.#") +
                    " seconds remaining.");
            }

            return command;
        }
    }
}

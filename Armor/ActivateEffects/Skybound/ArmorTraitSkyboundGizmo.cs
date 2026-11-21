
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

            // Original RimWorld Jump Pack icon
            ThingDef jumpPackDef =
                DefDatabase<ThingDef>.GetNamedSilentFail(
                    "Apparel_PackJump");

            Texture2D jumpIcon =
                jumpPackDef != null
                    ? jumpPackDef.uiIcon
                    : BaseContent.BadTex;

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

                            ArmorTraitSkyboundUtility.TryJump(
                                caster,
                                target.Cell,
                                quality,
                                state);
                        },
                        null,
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


using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitBerserkGizmo
    {
        private const float Range = 19.9f;

        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null || comp.State == null)
                return null;

            ArmorTraitState state = comp.State;

            // Confirmed vanilla Royalty Berserk icon.
            Texture2D icon =
                ContentFinder<Texture2D>.Get(
                    "UI/Abilities/Berserk",
                    false);

            if (icon == null)
                icon = BaseContent.BadTex;

            Command_Action command = new Command_Action
            {
                defaultLabel = "Berserk",
                icon = icon,

                defaultDesc =
                    "Forces a target into a berserk mental state.\n\n" +
                    "Base duration: " +
                    (quality == QualityCategory.Legendary
                        ? "30"
                        : "20") +
                    " seconds\n" +
                    "Duration scales with target psychic sensitivity " +
                    "(maximum 200%).\n" +
                    "Range: " + Range.ToString("0.#") + " cells\n" +
                    "Charges: " + state.Charges + "/" +
                    state.MaxCharges,

                action = delegate
                {
                    Apparel apparel = comp.parent as Apparel;

                    Pawn caster =
                        apparel != null ? apparel.Wearer : null;

                    if (caster == null ||
                        caster.Dead ||
                        !caster.Spawned ||
                        state.Charges <= 0)
                    {
                        return;
                    }

                    TargetingParameters targetParams =
                        new TargetingParameters
                        {
                            canTargetPawns = true,
                            canTargetBuildings = false,
                            canTargetItems = false,
                            canTargetLocations = false,
                            canTargetSelf = true,
                            canTargetAnimals = true
                        };

                    Find.Targeter.BeginTargeting(
                        targetParams,
                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn = target.Pawn;

                            if (apparel.Wearer != caster ||
                                !caster.Spawned ||
                                targetPawn == null ||
                                !caster.Position.InHorDistOf(
                                    targetPawn.Position,
                                    Range))
                            {
                                return;
                            }

                            ArmorTraitBerserkUtility.TryApply(
                                caster,
                                targetPawn,
                                quality,
                                state);
                        },
                        null,
                        delegate (LocalTargetInfo target)
                        {
                            Pawn targetPawn = target.Pawn;

                            return apparel.Wearer == caster &&
                                   caster.Spawned &&
                                   targetPawn != null &&
                                   caster.Position.InHorDistOf(
                                       targetPawn.Position,
                                       Range) &&
                                   ArmorTraitBerserkUtility.CanTarget(
                                       caster,
                                       targetPawn,
                                       state);
                        },
                        caster);
                }
            };

            if (state.Charges <= 0)
            {
                command.Disable("No charges available.");
            }

            return command;
        }
    }
}


using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitKindleGizmo
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
                ArmorTraitKindleUtility.GetRange(quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Kindle",
                defaultDesc =
                    "Ignites a flammable cell within range." +
                    "\n\nRange: " + range.ToString("0") +
                    " cells" +
                    "\nCooldown between uses: 5 seconds" +
                    "\nGoodwill penalty: " +
                    ArmorTraitKindleUtility
                        .GetGoodwillPenalty(quality),

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Kindle",
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
                                apparel.Wearer != pawn ||
                                !ArmorTraitKindleUtility.CanIgnite(
                                    pawn,
                                    target.Cell,
                                    quality,
                                    state))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_Kindle");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_Kindle");
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
                                ArmorTraitKindleUtility.CanIgnite(
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
                    "Wearer cannot activate Kindle.");
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

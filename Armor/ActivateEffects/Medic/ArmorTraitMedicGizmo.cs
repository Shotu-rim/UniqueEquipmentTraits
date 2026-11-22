
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitMedicGizmo
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
                ArmorTraitMedicUtility.GetRange(quality);

            float healingPool =
                ArmorTraitMedicUtility.GetHealingPool(quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Medic",

                defaultDesc =
                    "Releases medical nanites to repair injuries " +
                    "on a selected pawn." +
                    "\n\nHealing pool: " +
                    healingPool.ToString("0") + " HP" +
                    "\nRange: " +
                    range.ToString("0") + " cells" +
                    "\nMaximum charges: " +
                    (quality == QualityCategory.Legendary ? "2" : "1") +
                    "\nUnused healing capacity is lost.",

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Medic",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (pawn == null ||
                        pawn.Map == null ||
                        !state.HasAvailableCharge ||
                        state.IsOnCooldown)
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

                            if (targetPawn == null)
                                return;


                            if (apparel == null ||
                                apparel.Wearer != pawn ||
                                !ArmorTraitMedicUtility.CanHeal(
                                    pawn,
                                    targetPawn,
                                    quality,
                                    state))
                            {
                                return;
                            }

                            ArmorTraitDef trait =
                                DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                                    "UET_Medic");

                            if (trait == null)
                            {
                                Log.Error(
                                    "[UET] Missing ArmorTraitDef: UET_Medic");
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
                                ArmorTraitMedicUtility.CanHeal(
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
                    "Wearer cannot activate Medic.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Medic is on cooldown.");
            }

            return command;
        }
    }
}

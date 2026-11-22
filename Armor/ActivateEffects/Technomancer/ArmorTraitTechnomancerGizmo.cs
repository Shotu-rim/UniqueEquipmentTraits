
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitTechnomancerGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null ||
                comp.parent == null ||
                comp.State == null)
            {
                return null;
            }

            Apparel apparel = comp.parent as Apparel;

            if (apparel == null)
                return null;

            Pawn caster = apparel.Wearer;

            if (caster == null ||
                caster.Map == null ||
                caster.Dead)
            {
                return null;
            }

            ArmorTraitState state = comp.State;

            float radius =
                ArmorTraitTechnomancerUtility.GetRadius(
                    quality);

            float bonus =
                ArmorTraitTechnomancerUtility.GetCapacityBonus(
                    quality);

            int durationTicks =
                ArmorTraitTechnomancerUtility.GetDurationTicks(
                    quality);

            int cooldownTicks =
                ArmorTraitTechnomancerUtility.GetCooldownTicks();

            Command_Action command = new Command_Action
            {
                defaultLabel = "Mech Synch",

                defaultDesc =
                    "Emits a pulse that enhances Moving and " +
                    "Manipulation for friendly mechanoids " +
                    "controlled by a mechanitor.\n\n" +
                    "Capacity bonus: +" +
                    (bonus * 100f).ToString("0.##") +
                    "%\n" +
                    "Radius: " +
                    radius.ToString("0.#") +
                    " cells\n" +
                    "Duration: " +
                    (durationTicks / 60f).ToString("0.#") +
                    " seconds\n" +
                    "Cooldown: " +
                    (cooldownTicks / 60f).ToString("0.#") +
                    " seconds\n" +
                    "Charges: " +
                    state.Charges + "/" +
                    state.MaxCharges,

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Technomancer",
                    false),

                action = delegate
                {
                    if (apparel.Wearer != caster)
                        return;

                    if (!ArmorTraitTechnomancerUtility.CanActivate(
                        caster,
                        quality,
                        state))
                    {
                        return;
                    }

                    ArmorTraitDef trait =
                        DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                            "UET_Technomancer");

                    if (trait == null)
                    {
                        Log.Error("[UET] Missing ArmorTraitDef: UET_Technomancer");
                        return;
                    }

                    ArmorTraitCastUtility.TryStartCast(
                        caster,
                        apparel,
                        trait);
                }

            };

            if (state.MaxCharges <= 0)
            {
                command.Disable(
                    "No charges configured.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Ability is on cooldown: " +
                    (state.RemainingCooldownTicks / 60f)
                        .ToString("0.#") +
                    " seconds.");
            }
            else if (!ArmorTraitTechnomancerUtility.CanActivate(
                caster,
                quality,
                state))
            {
                command.Disable(
                    "Cannot activate Technomancer.");
            }

            return command;
        }
    }
}

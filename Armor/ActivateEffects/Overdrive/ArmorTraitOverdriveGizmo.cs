
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitOverdriveGizmo
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

            float speedBonus =
                ArmorTraitOverdriveUtility.GetSpeedBonus(
                    quality);

            float penalty =
                ArmorTraitOverdriveUtility.GetExhaustedPenalty(
                    quality);

            int duration =
                ArmorTraitOverdriveUtility.GetDurationTicks(
                    quality);

            int exhaustion =
                ArmorTraitOverdriveUtility.GetExhaustedTicks(
                    quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Overdrive",

                defaultDesc =
                    "Temporarily increases movement and attack speed, " +
                    "followed by severe exhaustion." +
                    "\n\nMovement and attack speed: +" +
                    (speedBonus * 100f).ToString("0.#") + "%" +
                    "\nDuration: " +
                    (duration / 60f).ToString("0.#") +
                    " seconds" +
                    "\n\nExhausted:" +
                    "\nMoving: -" +
                    (penalty * 100f).ToString("0.#") +
                    " percentage points" +
                    "\nManipulation: -" +
                    (penalty * 100f).ToString("0.#") +
                    " percentage points" +
                    "\nDuration: " +
                    (exhaustion / 60f).ToString("0.#") +
                    " seconds" +
                    "\n\nCharges: " +
                    state.Charges + "/" + state.MaxCharges +
                    "\nCharge regeneration: " +
                    (ArmorTraitState.GetChargeRegenTicks(
                        quality) / 60000f).ToString("0.#") +
                    " days" +
                    "\nCooldown: " +
                    (ArmorTraitOverdriveUtility.GetCooldownTicks()
                        / 60f).ToString("0.#") +
                    " seconds" +
                    "\n\nCannot be activated while Overdrive " +
                    "or Exhausted is active.",

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_Overdrive",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    if (apparel == null ||
                        apparel.Wearer != pawn ||
                        !ArmorTraitOverdriveUtility.CanActivate(
                            pawn,
                            quality,
                            state))
                    {
                        return;
                    }


                    ArmorTraitDef trait =
                        DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                            "UET_Overdrive");

                    if (trait == null)
                    {
                        Log.Error(
                            "[UET] Missing ArmorTraitDef: UET_Overdrive");
                        return;
                    }

                    ArmorTraitCastUtility.TryStartCast(
                        pawn,
                        apparel,
                        trait);

                }
            };

            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                !pawn.Spawned)
            {
                command.Disable(
                    "Wearer cannot activate Overdrive.");
            }
            else if (
                ArmorTraitOverdriveUtility.HasBlockingHediff(pawn))
            {
                command.Disable(
                    "Overdrive or Exhausted is active.");
            }
            else if (state.IsEffectActive)
            {
                command.Disable(
                    "Overdrive is active.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Overdrive is on cooldown.");
            }
            else if (!state.HasAvailableCharge)
            {
                command.Disable(
                    "No charges available.");
            }

            return command;
        }
    }
}

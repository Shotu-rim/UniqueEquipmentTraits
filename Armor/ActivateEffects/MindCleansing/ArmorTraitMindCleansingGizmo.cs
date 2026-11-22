
using RimWorld;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitMindCleansingGizmo
    {
        public static Gizmo Create(
            CompArmorTraits comp,
            QualityCategory quality)
        {
            if (comp == null || comp.State == null)
                return null;

            ArmorTraitState state = comp.State;

            int paralysisTicks =
                ArmorTraitMindCleansingUtility.GetParalysisTicks(quality);

            int cooldownTicks =
                ArmorTraitMindCleansingUtility.GetCooldownTicks(quality);

            Command_Action command = new Command_Action
            {
                defaultLabel = "Mind Cleansing",

                defaultDesc =
                    "Instantly removes all accumulated neural heat, " +
                    "but temporarily paralyzes the wearer.\n\n" +
                    "Paralysis: " +
                    (paralysisTicks / 60f).ToString("0.#") +
                    " seconds\n" +
                    "Cooldown: " +
                    (cooldownTicks / 2500f).ToString("0.#") +
                    " hours",

                icon = ContentFinder<Texture2D>.Get(
                    "UI/Commands/UET_MindCleansing",
                    false) ?? BaseContent.BadTex,

                action = delegate
                {
                    Apparel apparel = comp.parent as Apparel;

                    Pawn wearer = apparel != null
                        ? apparel.Wearer
                        : null;

                    if (wearer == null)
                        return;


                    if (apparel.Wearer != wearer ||
                        wearer.Dead ||
                        wearer.Downed ||
                        wearer.psychicEntropy == null ||
                        wearer.psychicEntropy.EntropyValue <= 0f ||
                        state.IsEffectActive ||
                        state.IsOnCooldown)
                    {
                        return;
                    }

                    ArmorTraitDef trait =
                        DefDatabase<ArmorTraitDef>.GetNamedSilentFail(
                            "UET_MindCleansing");

                    if (trait == null)
                    {
                        Log.Error(
                            "[UET] Missing ArmorTraitDef: UET_MindCleansing");
                        return;
                    }

                    ArmorTraitCastUtility.TryStartCast(
                        wearer,
                        apparel,
                        trait);

                }
            };

            Apparel wornApparel = comp.parent as Apparel;

            Pawn pawn = wornApparel != null
                ? wornApparel.Wearer
                : null;

            if (state.IsEffectActive)
            {
                command.Disable("Mind cleansing is active.");
            }
            else if (state.IsOnCooldown)
            {
                command.Disable(
                    "Cooldown: " +
                    (state.RemainingCooldownTicks / 2500f)
                        .ToString("0.#") +
                    " hours remaining.");
            }
            else if (pawn == null ||
                     pawn.Dead ||
                     pawn.Downed ||
                     pawn.psychicEntropy == null)
            {
                command.Disable("Wearer cannot activate Mind Cleansing.");
            }
            else if (pawn.psychicEntropy.EntropyValue <= 0f)
            {
                command.Disable("No neural heat to cleanse.");
            }

            return command;
        }
    }
}

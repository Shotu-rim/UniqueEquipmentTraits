
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public sealed class DisruptiveEffect : UETEffect
    {
        private const float NeuralHeatMultiplier = 60f;
        private const float ReferenceNeuralHeatMax = 80f;
        private const float PsychicSensitivityExponent = 0.25f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null)
                return;

            Pawn victim = context.Victim;

            if (!CanReceiveNeuralHeat(victim))
                return;

            // Actual Psychic Sensitivity, including modifiers.
            float sensitivity =
                victim.GetStatValue(StatDefOf.PsychicSensitivity);

            // Completely psychically insensitive targets
            // are immune to Disruptive.
            if (sensitivity <= 0f)
                return;

            Pawn_PsychicEntropyTracker tracker =
                victim.psychicEntropy;

            // Maximum Neural Heat of the victim.
            float maxHeat = tracker.MaxEntropy;

            if (maxHeat <= 0f)
                return;

            // Heat = 60 * EffectStrength
            //      * (MaxHeat / 80)
            //      * Sensitivity^0.25
            float amount =
                NeuralHeatMultiplier *
                context.EffectStrength *
                (maxHeat / ReferenceNeuralHeatMax) *
                UnityEngine.Mathf.Pow(
                    sensitivity,
                    PsychicSensitivityExponent);

            if (amount <= 0f)
                return;

            float before = tracker.EntropyValue;

            bool applied = tracker.TryAddEntropy(
                amount,
                source: context.Attacker,
                scale: false,
                overLimit: true);

            if (!applied)
                return;

            float gained = tracker.EntropyValue - before;

            if (gained <= 0f)
                return;

            context.Report(
                $"{context.Attacker?.LabelShortCap ?? "Unknown"} " +
                $"disrupted {victim.LabelShortCap}'s psychic abilities, " +
                $"generating {gained:0.#} neural heat " +
                $"({before:0.#} → {tracker.EntropyValue:0.#}).");
        }

        private static bool CanReceiveNeuralHeat(Pawn pawn)
        {
            if (pawn == null || pawn.Dead ||
                !ModsConfig.RoyaltyActive ||
                pawn.psychicEntropy == null ||
                pawn.abilities == null)
            {
                return false;
            }

            // Standard psylink is sufficient.
            if (pawn.psychicEntropy.Psylink != null)
                return true;

            // Also recognize abilities that consume Neural Heat,
            // even when the pawn has no standard psylink.
            foreach (Ability ability
                in pawn.abilities.AllAbilitiesForReading)
            {
                if (ability?.def != null &&
                    ability.def.IsPsycast &&
                    ability.def.EntropyGain > 0f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

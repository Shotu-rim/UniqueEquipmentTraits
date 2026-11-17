
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

            // Sensitivity with modifiers
            float sensitivity =
                victim.GetStatValue(StatDefOf.PsychicSensitivity);

            // Immunity
            if (sensitivity <= 0f)
                return;

            Pawn_PsychicEntropyTracker tracker =
                victim.psychicEntropy;

            // Max victim's Neural Heat
            float maxHeat = tracker.MaxEntropy;

            if (maxHeat <= 0f)
                return;

            // Heat = 60 * EffectStrength * (MaxHeat / 80) * Sensitivity^0.25 - example
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

            // For standard psylink
            if (pawn.psychicEntropy.Psylink != null)
                return true;

            // Identify abilities consuming Neural Heat. No psylink needed
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

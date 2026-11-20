
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitRegenerationTracker : GameComponent
    {
        private Dictionary<Pawn, float> healingProgress =
            new Dictionary<Pawn, float>();

        private Dictionary<Pawn, int> lastUpdateTicks =
            new Dictionary<Pawn, int>();

        private Dictionary<Pawn, float> cachedHealingRates =
            new Dictionary<Pawn, float>();

        private List<Pawn> progressPawnKeys;
        private List<float> progressValues;

        public ArmorTraitRegenerationTracker(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(
                ref healingProgress,
                "UET_RegenerationProgress",
                LookMode.Reference,
                LookMode.Value,
                ref progressPawnKeys,
                ref progressValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (healingProgress == null)
                    healingProgress =
                        new Dictionary<Pawn, float>();

                // Timing and rate caches are session-local.
                lastUpdateTicks =
                    new Dictionary<Pawn, int>();

                cachedHealingRates =
                    new Dictionary<Pawn, float>();

                progressPawnKeys = null;
                progressValues = null;
            }
        }

        public void Advance(Pawn pawn, int delta)
        {
            if (pawn == null || pawn.Dead || delta <= 0)
                return;

            int currentTick = Find.TickManager.TicksGame;

            if (pawn.Suspended)
            {
                lastUpdateTicks.Remove(pawn);
                return;
            }

            float rate;

            if (!cachedHealingRates.TryGetValue(
                pawn, out rate))
            {
                rate =
                    ArmorTraitRegenerationUtility
                        .GetHealingPerTick(pawn);

                cachedHealingRates[pawn] = rate;
            }

            if (rate <= 0f)
            {
                lastUpdateTicks.Remove(pawn);
                return;
            }

            int lastTick;
            int elapsedTicks;

            if (lastUpdateTicks.TryGetValue(
                pawn, out lastTick))
            {
                elapsedTicks = currentTick - lastTick;

                if (elapsedTicks <= 0)
                    return;
            }
            else
            {
                elapsedTicks = delta;
            }

            lastUpdateTicks[pawn] = currentTick;

            if (elapsedTicks > 2500)
                elapsedTicks = 2500;

            ApplyHealing(pawn, rate * elapsedTicks);
        }

        public void NotifyApparelChanged(Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                return;

            float oldRate;

            if (cachedHealingRates.TryGetValue(
                pawn, out oldRate))
            {
                int lastTick;

                if (oldRate > 0f &&
                    lastUpdateTicks.TryGetValue(
                        pawn, out lastTick))
                {
                    int currentTick =
                        Find.TickManager.TicksGame;

                    int elapsedTicks =
                        currentTick - lastTick;

                    if (elapsedTicks > 0)
                    {
                        if (elapsedTicks > 2500)
                            elapsedTicks = 2500;

                        ApplyHealing(
                            pawn,
                            oldRate * elapsedTicks);
                    }
                }
            }

            float newRate =
                ArmorTraitRegenerationUtility
                    .GetHealingPerTick(pawn);

            cachedHealingRates[pawn] = newRate;

            if (newRate > 0f)
            {
                lastUpdateTicks[pawn] =
                    Find.TickManager.TicksGame;
            }
            else
            {
                lastUpdateTicks.Remove(pawn);
            }
        }

        private void ApplyHealing(
            Pawn pawn,
            float healingAmount)
        {
            if (healingAmount <= 0f)
                return;

            float progress;

            if (!healingProgress.TryGetValue(
                pawn, out progress))
            {
                progress = 0f;
            }

            progress += healingAmount;

            while (progress >= 1f)
            {
                if (!ArmorTraitRegenerationHealing
                    .TryHealRandomInjury(pawn))
                {
                    progress = 0f;
                    break;
                }

                progress -= 1f;
            }

            if (progress > 0f)
                healingProgress[pawn] = progress;
            else
                healingProgress.Remove(pawn);
        }
    }
}

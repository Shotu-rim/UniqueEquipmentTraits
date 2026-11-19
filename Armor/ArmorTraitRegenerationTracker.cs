
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

        private List<Pawn> tickPawnKeys;
        private List<int> tickValues;

        private List<Pawn> ratePawnKeys;
        private List<float> rateValues;

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

            Scribe_Collections.Look(
                ref lastUpdateTicks,
                "UET_RegenerationLastTicks",
                LookMode.Reference,
                LookMode.Value,
                ref tickPawnKeys,
                ref tickValues);

            Scribe_Collections.Look(
                ref cachedHealingRates,
                "UET_RegenerationCachedRates",
                LookMode.Reference,
                LookMode.Value,
                ref ratePawnKeys,
                ref rateValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (healingProgress == null)
                    healingProgress = new Dictionary<Pawn, float>();

                if (lastUpdateTicks == null)
                    lastUpdateTicks = new Dictionary<Pawn, int>();

                if (cachedHealingRates == null)
                    cachedHealingRates = new Dictionary<Pawn, float>();

                progressPawnKeys = null;
                progressValues = null;
                tickPawnKeys = null;
                tickValues = null;
                ratePawnKeys = null;
                rateValues = null;
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

            if (!cachedHealingRates.TryGetValue(pawn, out rate))
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

            if (lastUpdateTicks.TryGetValue(pawn, out lastTick))
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

            // Safety cap against excessive catch-up.
            if (elapsedTicks > 2500)
                elapsedTicks = 2500;

            ApplyHealing(pawn, rate * elapsedTicks);
        }

        public void NotifyApparelChanged(Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                return;

            // Settle elapsed time using the previous rate.
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

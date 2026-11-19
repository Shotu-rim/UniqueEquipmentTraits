
using System;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Stores persistent runtime state for one armor trait
    /// assigned to one individual apparel item.
    /// </summary>
    public sealed class ArmorTraitState : IExposable
    {
        private const int BaseChargeRegenTicks = 60000;
        private const float LegendaryRegenReduction = 0.20f;
        private string lastTriggeredPartLabel;

        private int charges;
        private int maxCharges;
        private int cooldownEndTick = -1;
        private int nextChargeTick = -1;
        private int activeEffectEndTick = -1;
        public string LastTriggeredPartLabel
        {
            get { return lastTriggeredPartLabel; }
        }

        public void SetLastTriggeredPart(BodyPartRecord part)
        {
            lastTriggeredPartLabel =
                part != null
                    ? ArmorTraitAnatomy.GetDisplayCategory(part)
                    : null;
        }
        public int RemainingActiveTicks
        {
            get
            {
                return Math.Max(
                    0,
                    activeEffectEndTick - CurrentTick);
            }
        }

        public int Charges
        {
            get
            {
                UpdateChargeRegeneration();
                return charges;
            }
        }

        public int MaxCharges
        {
            get { return maxCharges; }
        }

        public int CooldownEndTick
        {
            get { return cooldownEndTick; }
        }

        public int NextChargeTick
        {
            get
            {
                UpdateChargeRegeneration();
                return nextChargeTick;
            }
        }

        public int ActiveEffectEndTick
        {
            get { return activeEffectEndTick; }
        }

        public bool UsesCharges
        {
            get { return maxCharges > 0; }
        }

        public bool HasAvailableCharge
        {
            get
            {
                UpdateChargeRegeneration();

                return maxCharges <= 0 || charges > 0;
            }
        }

        public bool IsOnCooldown
        {
            get
            {
                return !IsEffectActive &&
                    cooldownEndTick > CurrentTick;
            }
        }


        public int RemainingCooldownTicks
        {
            get
            {
                if (IsEffectActive)
                    return Math.Max(
                        0,
                        cooldownEndTick - activeEffectEndTick);

                return Math.Max(
                    0,
                    cooldownEndTick - CurrentTick);
            }
        }


        public bool IsEffectActive
        {
            get
            {
                return activeEffectEndTick > CurrentTick;
            }
        }

        public int RemainingChargeTicks
        {
            get
            {
                UpdateChargeRegeneration();

                if (nextChargeTick < 0)
                    return 0;

                return Math.Max(
                    0,
                    nextChargeTick - CurrentTick);
            }
        }

        private int CurrentTick
        {
            get
            {
                return Find.TickManager != null
                    ? Find.TickManager.TicksGame
                    : 0;
            }
        }

        // Counts charge regen time. Legends regen faster
        public static int GetChargeRegenTicks(
            QualityCategory quality)
        {
            float multiplier =
                quality == QualityCategory.Legendary
                    ? 1f - LegendaryRegenReduction
                    : 1f;

            return Math.Max(
                1,
                (int)Math.Round(
                    BaseChargeRegenTicks * multiplier));
        }

        // Takes charges from a trait, save/load safe
        public void ConfigureCharges(
            int maximum,
            QualityCategory quality)
        {
            maxCharges = Math.Max(0, maximum);
            charges = maxCharges;
            nextChargeTick = -1;

            chargeRegenTicks =
                GetChargeRegenTicks(quality);
        }

        private int chargeRegenTicks;

        // Restore charges since last update of charge state
        public void UpdateChargeRegeneration()
        {
            if (maxCharges <= 0 ||
                charges >= maxCharges ||
                nextChargeTick < 0 ||
                chargeRegenTicks <= 0)
            {
                return;
            }

            int now = CurrentTick;

            if (now < nextChargeTick)
                return;

            long elapsed =
                (long)now - nextChargeTick;

            long completedPeriods =
                1L + elapsed / chargeRegenTicks;

            int missingCharges =
                maxCharges - charges;

            int restored =
                (int)Math.Min(
                    completedPeriods,
                    missingCharges);

            charges += restored;

            if (charges >= maxCharges)
            {
                nextChargeTick = -1;
            }
            else
            {
                nextChargeTick =
                    (int)((long)nextChargeTick +
                    (long)restored * chargeRegenTicks);
            }
        }

        // Charge consuming, charge regen starts after 1 charge, traits without charge not included
        public bool TryConsumeCharge()
        {
            UpdateChargeRegeneration();

            if (maxCharges <= 0)
                return true;

            if (charges <= 0)
                return false;

            charges--;

            if (nextChargeTick < 0 &&
                chargeRegenTicks > 0)
            {
                nextChargeTick =
                    CurrentTick + chargeRegenTicks;
            }

            return true;
        }

        public void StartCooldown(int durationTicks)
        {
            if (durationTicks <= 0)
                return;

            cooldownEndTick =
                CurrentTick + durationTicks;
        }

        public void StartActiveEffect(int durationTicks)
        {
            if (durationTicks <= 0)
                return;

            activeEffectEndTick =
                CurrentTick + durationTicks;
        }

        // Setting time for another charge count
        public void SetNextChargeTick(int tick)
        {
            nextChargeTick = tick;
        }

        // Charge regen and new timeline for next regen
        public void RestoreCharge()
        {
            UpdateChargeRegeneration();

            if (maxCharges <= 0 ||
                charges >= maxCharges)
            {
                return;
            }

            charges++;

            if (charges >= maxCharges)
                nextChargeTick = -1;
        }

        public void StartActivation(
            int activeDurationTicks,
            int cooldownDurationTicks)
        {
            int now = CurrentTick;

            int activeDuration =
                Math.Max(0, activeDurationTicks);

            int cooldownDuration =
                Math.Max(0, cooldownDurationTicks);

            activeEffectEndTick =
                activeDuration > 0
                    ? now + activeDuration
                    : -1;

            // Cooldown begins when the active effect ends
            cooldownEndTick =
                cooldownDuration > 0
                    ? now + activeDuration + cooldownDuration
                    : -1;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(
                ref lastTriggeredPartLabel,
                "lastTriggeredPartLabel",
                null);

            Scribe_Values.Look(
                ref charges,
                "charges",
                0);

            Scribe_Values.Look(
                ref maxCharges,
                "maxCharges",
                0);

            Scribe_Values.Look(
                ref cooldownEndTick,
                "cooldownEndTick",
                -1);

            Scribe_Values.Look(
                ref nextChargeTick,
                "nextChargeTick",
                -1);

            Scribe_Values.Look(
                ref activeEffectEndTick,
                "activeEffectEndTick",
                -1);

            Scribe_Values.Look(
                ref chargeRegenTicks,
                "chargeRegenTicks",
                0);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                maxCharges = Math.Max(0, maxCharges);

                charges = Math.Max(
                    0,
                    Math.Min(charges, maxCharges));

                if (maxCharges <= 0 ||
                    charges >= maxCharges)
                {
                    nextChargeTick = -1;
                }
            }
        }
    }
}

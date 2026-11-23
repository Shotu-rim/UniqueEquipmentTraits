
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitMindCleansingUtility
    {
        private const int MasterworkParalysisTicks = 2400;
        private const int LegendaryParalysisTicks = 1800;

        private const int MasterworkCooldownTicks = 20000;
        private const int LegendaryCooldownTicks = 15000;

        public static int GetParalysisTicks(QualityCategory quality)
        {
            return quality == QualityCategory.Legendary
                ? LegendaryParalysisTicks
                : MasterworkParalysisTicks;
        }

        public static int GetCooldownTicks(QualityCategory quality)
        {
            return quality == QualityCategory.Legendary
                ? LegendaryCooldownTicks
                : MasterworkCooldownTicks;
        }

        public static bool TryActivate(
            Pawn pawn,
            QualityCategory quality,
            ArmorTraitState state)
        {

            if (!ModsConfig.RoyaltyActive)
                return false;

            if (pawn == null ||
                pawn.Dead ||
                pawn.Downed ||
                pawn.psychicEntropy == null ||
                pawn.health == null ||
                state == null ||
                state.IsEffectActive ||
                state.IsOnCooldown)
            {
                return false;
            }

            if (pawn.psychicEntropy.EntropyValue <= 0f)
                return false;

            HediffDef paralysisDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(
                    "UET_MindCleansingParalysis");

            HediffDef vanillaAbasia =
                DefDatabase<HediffDef>.GetNamedSilentFail("Abasia");

            if (paralysisDef == null ||
                pawn.health.hediffSet.HasHediff(paralysisDef) ||
                (vanillaAbasia != null &&
                 pawn.health.hediffSet.HasHediff(vanillaAbasia)))
            {
                return false;
            }

            int paralysisTicks = GetParalysisTicks(quality);
            int cooldownTicks = GetCooldownTicks(quality);

            Hediff paralysis = pawn.health.AddHediff(paralysisDef);

            if (paralysis == null)
                return false;

            HediffComp_Disappears disappears =
                paralysis.TryGetComp<HediffComp_Disappears>();

            if (disappears == null)
            {
                pawn.health.RemoveHediff(paralysis);
                return false;
            }

            disappears.ticksToDisappear = paralysisTicks;

            pawn.psychicEntropy.RemoveAllEntropy();

            state.StartActivation(paralysisTicks, cooldownTicks);

            return true;
        }
    }
}

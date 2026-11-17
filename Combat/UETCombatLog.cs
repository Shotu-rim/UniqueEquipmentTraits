using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    /// <summary>
    /// Records UET weapon trait activations and their reported effects
    /// in RimWorld's native combat log.
    /// Combines effect reports into a single entry for each recorded activation.
    /// Used internally by the UET effect reporting system.
    /// </summary>
    internal static class UETCombatLog
    {
        internal static void Record(
            WeaponTraitDef trait,
            UETEffectContext context)
        {
            if (trait == null || context == null)
                return;

            if (context.Reports == null ||
                context.Reports.Count == 0)
                return;

            if (Current.Game == null ||
                Find.BattleLog == null)
                return;

            Pawn attacker = context.Attacker;
            Pawn victim = context.Victim;

            if (attacker == null || victim == null)
                return;

            string traitName = trait.LabelCap;

            string results = string.Join(
                " ",
                context.Reports);

            string description =
                $"{traitName} activated: " +
                $"{attacker.LabelShort} → " +
                $"{victim.LabelShort}. " +
                results;

            UETCombatLogEntry entry =
                new UETCombatLogEntry(
                    attacker,
                    victim,
                    description);



            Find.BattleLog.Add(entry);
        }
    }
}

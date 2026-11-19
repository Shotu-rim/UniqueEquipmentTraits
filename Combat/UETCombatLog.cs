using RimWorld;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.Effects
{
    /// <summary>
    /// Records UET weapon and armor trait effects
    /// in RimWorld's native combat log.
    /// Weapon effects are grouped by activation,
    /// while armor effects provide their own descriptions.
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

        internal static void Record(
            ArmorTraitDef trait,
            Pawn wearer,
            string description,
            Pawn otherPawn = null)
        {
            if (trait == null ||
                wearer == null ||
                string.IsNullOrWhiteSpace(description))
            {
                return;
            }

            if (Current.Game == null ||
                Find.BattleLog == null)
            {
                return;
            }

            UETCombatLogEntry entry =
                new UETCombatLogEntry(
                    wearer,
                    otherPawn,
                    description);

            Find.BattleLog.Add(entry);
        }

    }
}

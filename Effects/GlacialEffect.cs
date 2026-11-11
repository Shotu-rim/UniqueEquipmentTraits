
using RimWorld;
using System.Collections.Generic;
using Verse;
using static Verse.DamageWorker;

namespace UniqueEquipmentTraits.Effects
{
    public class GlacialEffect : UETEffect
    {
        private const float FrostbiteMultiplier = 10f;

        public override void Apply(UETEffectContext context)
        {
            if (context == null || context.Victim == null)
                return;

            Pawn victim = context.Victim;

            if (victim.Dead || victim.health == null)
                return;

            // Glacial nie działa na mechanoidy.
            if (victim.RaceProps == null ||
                victim.RaceProps.IsMechanoid)
                return;

            float frostbiteDamage =
                context.EffectStrength * FrostbiteMultiplier;

            if (frostbiteDamage <= 0f)
                return;

            DamageDef frostbiteDef =
                DefDatabase<DamageDef>.GetNamedSilentFail("Frostbite");

            if (frostbiteDef == null)
            {
                Log.Error("[UET] Missing DamageDef: Frostbite");
                return;
            }

            if (context.HitParts == null)
                return;

            int partsAffected = 0;
            List<string> affectedParts = new List<string>();

            foreach (BodyPartRecord part in context.HitParts)
            {
                if (part == null)
                    continue;

                // Nie odmrażamy brakujących części ciała.
                if (victim.health.hediffSet.PartIsMissing(part))
                    continue;

                DamageInfo dinfo = new DamageInfo(
                    frostbiteDef,
                    frostbiteDamage,
                    0f,
                    -1f,
                    context.Attacker
                );

                // Odmrożenie dotyczy dokładnie trafionej części.
                dinfo.SetHitPart(part);

                // Zimno przenika przez pancerz.
                dinfo.SetIgnoreArmor(true);

                // Bez rozprzestrzeniania obrażeń na inne części.
                dinfo.SetAllowDamagePropagation(false);

                DamageResult result = victim.TakeDamage(dinfo);

                if (result != null && result.totalDamageDealt > 0f)
                {
                    partsAffected++;
                    affectedParts.Add(part.LabelCap);
                }
            }

            if (partsAffected > 0)
            {

                context.Report(
                    $"{context.Attacker.LabelShort} inflicted frostbite on " +
                    $"{context.Victim.LabelShort}, affecting {partsAffected} " +
                    $"body part(s) with {frostbiteDamage:F1} base damage each: " +
                    $"{string.Join(", ", affectedParts)}.");

            }
        }
    }
}

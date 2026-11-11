
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public abstract class UETEffect
    {
        public abstract void Apply(UETEffectContext context);
    }

    public class UETEffectContext
    {
        private readonly List<string> reports =
            new List<string>();

        private readonly List<BodyPartRecord> hitParts =
            new List<BodyPartRecord>();

        private readonly Dictionary<BodyPartRecord, float> partDamage =
            new Dictionary<BodyPartRecord, float>();

        private readonly Dictionary<BodyPartRecord, HediffDef> partInjuryDefs =
             new Dictionary<BodyPartRecord, HediffDef>();

        public IReadOnlyDictionary<BodyPartRecord, HediffDef> PartInjuryDefs =>
           partInjuryDefs;

        public Pawn Attacker { get; }
        public Pawn Victim { get; }
        public ThingWithComps Equipment { get; }
        public QualityCategory Quality { get; }
        public float EffectStrength { get; }
        public float BleedingBeforeAttack { get; }
        public float DamageAmount { get; }

        public IReadOnlyList<string> Reports => reports;

        public IReadOnlyList<BodyPartRecord> HitParts => hitParts;

        public IReadOnlyDictionary<BodyPartRecord, float> PartDamage =>
            partDamage;


        public UETEffectContext(
            Pawn attacker,
            Pawn victim,
            ThingWithComps equipment,
            QualityCategory quality,
            float effectStrength,
            float bleedingBeforeAttack = 0f,
            IEnumerable<BodyPartRecord> hitParts = null,
            float damageAmount = 0f,
            IReadOnlyDictionary<BodyPartRecord, float> partDamage = null,
            IReadOnlyDictionary<BodyPartRecord, HediffDef> partInjuryDefs = null)
        {
            Attacker = attacker;
            Victim = victim;
            Equipment = equipment;
            Quality = quality;
            EffectStrength = effectStrength;
            BleedingBeforeAttack = bleedingBeforeAttack;
            DamageAmount = damageAmount;

            if (hitParts != null)
            {
                foreach (BodyPartRecord part in hitParts)
                {
                    if (part != null && !this.hitParts.Contains(part))
                        this.hitParts.Add(part);
                }
            }

            if (partDamage != null)
            {
                foreach (KeyValuePair<BodyPartRecord, float> entry
                    in partDamage)
                {
                    if (entry.Key == null || entry.Value <= 0f)
                        continue;

                    this.partDamage[entry.Key] = entry.Value;
                }
            }

            if (partInjuryDefs != null)
            {
                foreach (KeyValuePair<BodyPartRecord, HediffDef> entry
                    in partInjuryDefs)
                {
                    if (entry.Key == null || entry.Value == null)
                        continue;

                    this.partInjuryDefs[entry.Key] = entry.Value;
                }
            }

        }

        public void Report(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            reports.Add(description);
        }
    }
}

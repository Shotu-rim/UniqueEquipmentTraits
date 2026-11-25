using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    /// <summary>
    /// Defines the base contract for UET equipment effects.
    /// </summary>
    public abstract class UETEffect
    {
        public abstract void Apply(UETEffectContext context);
    }

    /// <summary>
    /// Provides attack and equipment information required by UET effects.
    /// Stores both physical contact and actual damage information.
    /// </summary>
    public class UETEffectContext
    {
        private readonly List<string> reports =
            new List<string>();

        private readonly List<BodyPartRecord> hitParts =
            new List<BodyPartRecord>();

        private readonly List<BodyPartRecord> contactParts =
            new List<BodyPartRecord>();

        private readonly Dictionary<BodyPartRecord, float> partDamage =
            new Dictionary<BodyPartRecord, float>();

        private readonly Dictionary<BodyPartRecord, HediffDef> partInjuryDefs =
            new Dictionary<BodyPartRecord, HediffDef>();

        public Pawn Attacker { get; }
        public Pawn Victim { get; }
        public ThingWithComps Equipment { get; }
        public QualityCategory Quality { get; }
        public float EffectStrength { get; }
        public float BleedingBeforeAttack { get; }

        // Actual damage dealt to the victim
        public float DamageAmount { get; }

        // Physical contact before armor mitigation
        public float ContactDamageAmount { get; }

        public bool ContactMade =>
            contactParts.Count > 0;

        public IReadOnlyList<BodyPartRecord> ContactParts =>
            contactParts;

        public IReadOnlyList<string> Reports =>
            reports;

        public IReadOnlyList<BodyPartRecord> HitParts =>
            hitParts;

        public IReadOnlyDictionary<BodyPartRecord, float> PartDamage =>
            partDamage;

        public IReadOnlyDictionary<BodyPartRecord, HediffDef> PartInjuryDefs =>
            partInjuryDefs;

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
            IReadOnlyDictionary<BodyPartRecord, HediffDef> partInjuryDefs = null,
            float contactDamageAmount = 0f,
            IEnumerable<BodyPartRecord> contactParts = null)
        {
            Attacker = attacker;
            Victim = victim;
            Equipment = equipment;
            Quality = quality;
            EffectStrength = effectStrength;
            BleedingBeforeAttack = bleedingBeforeAttack;
            DamageAmount = damageAmount;
            ContactDamageAmount = contactDamageAmount;

            if (hitParts != null)
            {
                foreach (BodyPartRecord part in hitParts)
                {
                    if (part != null &&
                        !this.hitParts.Contains(part))
                    {
                        this.hitParts.Add(part);
                    }
                }
            }

            if (contactParts != null)
            {
                foreach (BodyPartRecord part in contactParts)
                {
                    if (part != null &&
                        !this.contactParts.Contains(part))
                    {
                        this.contactParts.Add(part);
                    }
                }
            }

            if (partDamage != null)
            {
                foreach (KeyValuePair<BodyPartRecord, float> entry
                    in partDamage)
                {
                    if (entry.Key == null ||
                        entry.Value <= 0f)
                        continue;

                    this.partDamage[entry.Key] = entry.Value;
                }
            }

            if (partInjuryDefs != null)
            {
                foreach (KeyValuePair<BodyPartRecord, HediffDef> entry
                    in partInjuryDefs)
                {
                    if (entry.Key == null ||
                        entry.Value == null)
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
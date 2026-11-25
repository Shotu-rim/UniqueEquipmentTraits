using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    /// <summary>
    /// Stores the state and results of a single melee attack.
    /// Tracks participants, weapon, contact damage, actual damage,
    /// affected body parts and trait activation state.
    /// </summary>
    internal sealed class MeleeAttackContext
    {
        public Pawn Attacker { get; }
        public Pawn Victim { get; }
        public ThingWithComps Weapon { get; }

        public bool DamageDealt { get; set; }
        public float DamageAmount { get; private set; }
        public float BleedingBeforeAttack { get; }

        // Contact observation
        public bool ContactObservationClosed { get; private set; }

        public bool ContactMade =>
            contactParts.Count > 0;

        public float ContactDamageAmount { get; private set; }

        public IEnumerable<BodyPartRecord> ContactParts =>
            contactParts;

        // Sundering
        public bool ArmorPiercingActivated { get; internal set; }

        public bool ArmorPiercingApplied { get; internal set; }

        public float ArmorPiercingStrength { get; internal set; }

        // Prismatic
        public RimWorld.WeaponTraitDef PrismaticSelectedTrait
        {
            get;
            internal set;
        }

        public bool PrismaticActivationRolled
        {
            get;
            internal set;
        }

        public bool PrismaticActivated
        {
            get;
            internal set;
        }

        private readonly HashSet<BodyPartRecord> hitParts =
            new HashSet<BodyPartRecord>();

        private readonly HashSet<BodyPartRecord> contactParts =
            new HashSet<BodyPartRecord>();

        private readonly Dictionary<BodyPartRecord, float> partDamage =
            new Dictionary<BodyPartRecord, float>();

        private readonly Dictionary<BodyPartRecord, HediffDef> partInjuryDefs =
            new Dictionary<BodyPartRecord, HediffDef>();

        private Dictionary<BodyPartRecord, float> originalPartDamage;

        private Dictionary<BodyPartRecord, HediffDef> originalPartInjuryDefs;

        public IReadOnlyDictionary<BodyPartRecord, float> OriginalPartDamage =>
            originalPartDamage;

        public IReadOnlyDictionary<BodyPartRecord, HediffDef> OriginalPartInjuryDefs =>
            originalPartInjuryDefs;

        public IEnumerable<BodyPartRecord> HitParts =>
            hitParts;

        public IReadOnlyDictionary<BodyPartRecord, float> PartDamage =>
            partDamage;

        public IReadOnlyDictionary<BodyPartRecord, HediffDef> PartInjuryDefs =>
            partInjuryDefs;

        /// <summary>
        /// Prevents subsequent trait-generated damage from being
        /// registered as original melee contact.
        /// </summary>
        public void CloseContactObservation()
        {
            ContactObservationClosed = true;
        }

        public void RegisterContactDamage(float amount)
        {
            if (ContactObservationClosed || amount <= 0f)
                return;

            ContactDamageAmount += amount;
        }

        public void RegisterContactPart(BodyPartRecord part)
        {
            if (ContactObservationClosed || part == null)
                return;

            contactParts.Add(part);
        }

        public void RegisterDamage(float amount)
        {
            if (amount <= 0f)
                return;

            DamageDealt = true;
            DamageAmount += amount;
        }

        public void RegisterHitPart(BodyPartRecord part)
        {
            if (part != null)
                hitParts.Add(part);
        }

        public void RegisterPartDamage(
            BodyPartRecord part,
            float amount,
            HediffDef injuryDef = null)
        {
            if (part == null || amount <= 0f)
                return;

            if (partDamage.TryGetValue(part, out float existingDamage))
                partDamage[part] = existingDamage + amount;
            else
                partDamage.Add(part, amount);

            if (injuryDef != null)
                partInjuryDefs[part] = injuryDef;
        }

        public void CaptureOriginalDamage()
        {
            if (originalPartDamage != null)
                return;

            originalPartDamage =
                new Dictionary<BodyPartRecord, float>(partDamage);

            originalPartInjuryDefs =
                new Dictionary<BodyPartRecord, HediffDef>(partInjuryDefs);
        }

        public MeleeAttackContext(
            Pawn attacker,
            Pawn victim,
            ThingWithComps weapon)
        {
            Attacker = attacker;
            Victim = victim;
            Weapon = weapon;

            BleedingBeforeAttack =
                victim != null &&
                victim.health != null &&
                !victim.Dead
                    ? victim.health.hediffSet.BleedRateTotal
                    : 0f;
        }
    }
}
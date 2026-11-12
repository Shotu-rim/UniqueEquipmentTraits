
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core
{
    internal sealed class MeleeAttackContext
    {
        public Pawn Attacker { get; }
        public Pawn Victim { get; }
        public ThingWithComps Weapon { get; }

        public bool DamageDealt { get; set; }
        public float DamageAmount { get; private set; }
        public float BleedingBeforeAttack { get; }
        public bool ArmorPiercingActivated { get; internal set; }

        public float ArmorPiercingStrength { get; internal set; }

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

        private readonly Dictionary<BodyPartRecord, float> partDamage =
            new Dictionary<BodyPartRecord, float>();

        private readonly Dictionary<BodyPartRecord, HediffDef> partInjuryDefs =
            new Dictionary<BodyPartRecord, HediffDef>();

        public IEnumerable<BodyPartRecord> HitParts => hitParts;

        public IReadOnlyDictionary<BodyPartRecord, float> PartDamage =>
            partDamage;

        public IReadOnlyDictionary<BodyPartRecord, HediffDef> PartInjuryDefs =>
            partInjuryDefs;

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


       


        public MeleeAttackContext(
            Pawn attacker,
            Pawn victim,
            ThingWithComps weapon)
        {
            Attacker = attacker;
            Victim = victim;
            Weapon = weapon;

            DamageDealt = false;
            DamageAmount = 0f;

            BleedingBeforeAttack =
                victim != null &&
                victim.health != null &&
                !victim.Dead
                    ? victim.health.hediffSet.BleedRateTotal
                    : 0f;
        }
    }
}

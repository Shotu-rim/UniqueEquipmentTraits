
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Effects
{
    public class Hediff_ShreddingBleeding : Hediff
    {
        private List<int> injuryIds = new List<int>();
        private List<float> multipliers = new List<float>();

        public override float BleedRate
        {
            get
            {
                if (pawn == null ||
                    pawn.Dead ||
                    pawn.health == null ||
                    !pawn.health.CanBleed)
                    return 0f;

                float extraBleeding = 0f;

                List<Hediff> hediffs =
                    pawn.health.hediffSet.hediffs;

                for (int i = 0; i < injuryIds.Count; i++)
                {
                    int id = injuryIds[i];
                    float multiplier = multipliers[i];

                    foreach (Hediff hediff in hediffs)
                    {
                        Hediff_Injury injury =
                            hediff as Hediff_Injury;

                        if (injury == null ||
                            injury.loadID != id)
                            continue;

                        extraBleeding +=
                            injury.BleedRate *
                            (multiplier - 1f);

                        break;
                    }
                }

                return extraBleeding;
            }
        }

        public void ApplyToInjury(
            Hediff_Injury injury,
            float multiplier)
        {
            if (injury == null || multiplier <= 1f)
                return;

            int index = injuryIds.IndexOf(injury.loadID);

            if (index >= 0)
            {
                multipliers[index] *= multiplier;
            }
            else
            {
                injuryIds.Add(injury.loadID);
                multipliers.Add(multiplier);
            }
        }

        public float GetMultiplier(int injuryId)
        {
            int index = injuryIds.IndexOf(injuryId);

            return index >= 0
                ? multipliers[index]
                : 1f;
        }


        public void TransferMergedInjury(
            int survivingId,
            int removedId)
        {
            if (survivingId == removedId)
                return;

            float highest = UnityEngine.Mathf.Max(
                GetMultiplier(survivingId),
                GetMultiplier(removedId));

            int removedIndex = injuryIds.IndexOf(removedId);

            if (removedIndex >= 0)
            {
                injuryIds.RemoveAt(removedIndex);
                multipliers.RemoveAt(removedIndex);
            }

            int survivingIndex = injuryIds.IndexOf(survivingId);

            if (survivingIndex >= 0)
            {
                multipliers[survivingIndex] = highest;
            }
            else if (highest > 1f)
            {
                injuryIds.Add(survivingId);
                multipliers.Add(highest);
            }
        }


        private void CleanupInjuries()
        {
            if (pawn == null || pawn.health == null)
                return;

            List<Hediff> hediffs =
                pawn.health.hediffSet.hediffs;

            for (int i = injuryIds.Count - 1; i >= 0; i--)
            {
                bool stillBleeding = false;

                foreach (Hediff hediff in hediffs)
                {
                    Hediff_Injury injury =
                        hediff as Hediff_Injury;

                    if (injury == null ||
                        injury.loadID != injuryIds[i])
                        continue;

                    stillBleeding = injury.BleedRate > 0f;
                    break;
                }

                if (!stillBleeding)
                {
                    injuryIds.RemoveAt(i);
                    multipliers.RemoveAt(i);
                }
            }
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (pawn == null ||
                pawn.Dead ||
                pawn.health == null)
                return;

            if (!pawn.IsHashIntervalTick(60, delta))
                return;

            CleanupInjuries();

            if (injuryIds.Count == 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(
                ref injuryIds,
                "shreddingInjuryIds",
                LookMode.Value);

            Scribe_Collections.Look(
                ref multipliers,
                "shreddingMultipliers",
                LookMode.Value);


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (injuryIds == null)
                    injuryIds = new List<int>();

                if (multipliers == null)
                    multipliers = new List<float>();

                int count = UnityEngine.Mathf.Min(
                    injuryIds.Count,
                    multipliers.Count);

                if (injuryIds.Count > count)
                    injuryIds.RemoveRange(
                        count,
                        injuryIds.Count - count);

                if (multipliers.Count > count)
                    multipliers.RemoveRange(
                        count,
                        multipliers.Count - count);
            }
        }
    }
}

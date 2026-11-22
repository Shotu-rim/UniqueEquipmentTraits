
namespace UniqueEquipmentTraits.Core.Armor
{
    /// <summary>
    /// Tracks damage absorbed by armor traits
    /// during a single ApplyToPawn invocation.
    /// </summary>
    internal sealed class ArmorDamageInterceptionContext
    {

        public float AbsorbedDamage { get; private set; }

        public float PendingAbsorbedDamage { get; private set; }

        public void RecordAbsorption(float damage)
        {
            if (damage <= 0f)
            return;
            MarkPartIntercepted();

            AbsorbedDamage += damage;
            PendingAbsorbedDamage += damage;
        }

        public bool LastPartIntercepted { get; private set; }

        public void BeginPartDamage()
        {
            LastPartIntercepted = false;
        }

        public void MarkPartIntercepted()
        {
            LastPartIntercepted = true;
        }


        public float ConsumePendingAbsorption()
        {
            float damage = PendingAbsorbedDamage;
            PendingAbsorbedDamage = 0f;

            return damage;
        }
        private float previousTotalDamageDealt;
    //    private float absorbedDamageAtIterationStart;
        public void BeginDamageIteration(float totalDamageDealt)
        {
            previousTotalDamageDealt = totalDamageDealt;
        }

        public float EndDamageIteration(float totalDamageDealt)
        {
            return GetDamageConsumedByIteration(
                totalDamageDealt,
                previousTotalDamageDealt);
        }

        public float GetDamageConsumedByIteration(
            float totalDamageDealt,
            float previousTotalDamageDealt)
        {
            float actualDamage =
                totalDamageDealt - previousTotalDamageDealt;

            if (actualDamage < 0f)
                actualDamage = 0f;

            float absorbedDamage =
                ConsumePendingAbsorption();

            return actualDamage + absorbedDamage;
        }

    }
}

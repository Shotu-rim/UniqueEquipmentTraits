
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorPassiveDamageUtility
    {
        private struct Candidate
        {
            public Apparel Apparel;
            public ArmorTraitDef Trait;
            public IArmorDamageInterceptor Interceptor;

            public Candidate(
                Apparel apparel,
                ArmorTraitDef trait,
                IArmorDamageInterceptor interceptor)
            {
                Apparel = apparel;
                Trait = trait;
                Interceptor = interceptor;
            }
        }

        public static bool TryInterceptDamage(
            Pawn pawn,
            float damage)
        {
            if (pawn == null ||
                pawn.apparel == null ||
                damage <= 0f)
            {
                return false;
            }

            List<Candidate> candidates =
                new List<Candidate>();

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel == null || apparel.Destroyed)
                    continue;

                CompArmorTraits comp =
                    apparel.TryGetComp<CompArmorTraits>();

                if (comp == null || comp.Traits == null)
                    continue;

                foreach (ArmorTraitDef trait in comp.Traits)
                {
                    if (trait == null ||
                        trait.activationType !=
                            ArmorTraitActivationType.Passive)
                    {
                        continue;
                    }

                    IArmorDamageInterceptor interceptor =
                        trait.Worker as IArmorDamageInterceptor;

                    if (interceptor == null)
                        continue;

                    if (!interceptor.CanInterceptDamage(
                        pawn,
                        apparel,
                        trait,
                        damage))
                    {
                        continue;
                    }

                    candidates.Add(
                        new Candidate(
                            apparel,
                            trait,
                            interceptor));
                }
            }

            if (candidates.Count == 0)
                return false;

            Candidate selected =
                candidates[Rand.Range(0, candidates.Count)];

            return selected.Interceptor.TryInterceptDamage(
                pawn,
                selected.Apparel,
                selected.Trait,
                damage);
        }

        internal static bool TryInterceptAndRecord(
            Pawn pawn,
            float damage)
        {
            if (damage <= 0f)
                return false;

            ArmorDamageInterceptionContext context =
                ArmorDamageInterceptionScope.Current;

            if (context == null)
                return false;

            if (!TryInterceptDamage(pawn, damage))
                return false;

            context.RecordAbsorption(damage);

            return true;
        }

    }
}


using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UniqueEquipmentTraits.Traits;

namespace UniqueEquipmentTraits.Effects
{
    internal static class UETEffectDispatcher
    {
        private static readonly Dictionary<Type, UETEffect> effectCache =
            new Dictionary<Type, UETEffect>();

        internal static bool TryApply(
            RimWorld.WeaponTraitDef trait,
            UETEffectContext context)
        {
            if (trait == null || context == null)
                return false;

            UETTraitExtension extension =
                trait.GetModExtension<UETTraitExtension>();

            if (extension == null || extension.effectClass == null)
                return false;

            Type effectType = extension.effectClass;

            if (!typeof(UETEffect).IsAssignableFrom(effectType) ||
                effectType.IsAbstract)
            {
                Log.Error(
                    $"[UET] Invalid effect class for trait " +
                    $"{trait.defName}: {effectType.FullName}");

                return false;
            }

            if (!effectCache.TryGetValue(effectType, out UETEffect effect))
            {
                try
                {
                    effect = (UETEffect)Activator.CreateInstance(effectType);
                    effectCache.Add(effectType, effect);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"[UET] Cannot create effect " +
                        $"{effectType.FullName}: {ex}");

                    return false;
                }
            }


            Log.Message(
                $"[UET DEBUG A] Before Apply: {trait.defName}");

            effect.Apply(context);

            Log.Message(
                $"[UET DEBUG B] After Apply: " +
                $"trait={trait.defName}, " +
                $"reports={context.Reports.Count}");

            foreach (string report in context.Reports)
            {
                Log.Message(
                    $"[UET DEBUG C] Report: {report}");
            }

            UETCombatLog.Record(trait, context);

            Log.Message(
                $"[UET DEBUG D] Record finished: {trait.defName}");


            return true;
        }
    }
}

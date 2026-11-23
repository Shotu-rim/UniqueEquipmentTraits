
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public class ArmorTraitWorker_Thorns : ArmorTraitWorker
    {
        public override bool CanActivate(
            ArmorTraitActivationContext context)
        {
            return context != null &&
                   context.Attacker != null &&
                   context.Defender != null &&
                   context.Apparel != null &&
                   context.Trait != null &&
                   context.InjurySeverity > 0f &&
                   !context.Attacker.Dead &&
                   context.Attacker.Spawned &&
                   UETThornsReflection.CanRegister;
        }

        public override void OnActivated(
            ArmorTraitActivationContext context)
        {
            if (!CanActivate(context))
                return;

            UETThornsReflection.Register(context);
        }
    }
}

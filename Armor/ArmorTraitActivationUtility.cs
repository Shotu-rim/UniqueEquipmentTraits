
using RimWorld;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public static class ArmorTraitActivationUtility
    {
        public static bool TryActivate(
            Pawn caster,
            Apparel apparel,
            LocalTargetInfo target)
        {
            if (caster == null ||
                apparel == null ||
                apparel.Destroyed ||
                apparel.Wearer != caster)
            {
                return false;
            }

            CompArmorTraits comp =
                apparel.TryGetComp<CompArmorTraits>();

            if (comp == null ||
                comp.State == null ||
                comp.Traits == null ||
                comp.Traits.Count == 0)
            {
                return false;
            }

            ArmorTraitDef trait = comp.Traits[0];

            if (trait == null)
                return false;

            QualityCategory quality;

            if (!apparel.TryGetQuality(out quality))
                return false;

            switch (trait.defName)
            {
                case "UET_Technomancer":
                    return ArmorTraitTechnomancerUtility.TryActivate(
                        caster,
                        quality,
                        comp.State);

                case "UET_ToxicCloud":
                    if (!target.IsValid)
                        return false;

                    return ArmorTraitToxicCloudUtility.TryRelease(
                        caster,
                        target.Cell,
                        quality,
                        comp.State);


                case "UET_Systemshock":
                    if (!target.IsValid)
                        return false;

                    return ArmorTraitSystemshockUtility.TryActivate(
                        caster,
                        target.Cell,
                        quality,
                        comp.State);

                case "UET_StunningPulse":
                    Pawn targetPawn = target.Thing as Pawn;

                    if (targetPawn == null)
                        return false;

                    return ArmorTraitStunningPulseUtility.TryActivate(
                        caster,
                        targetPawn,
                        quality,
                        comp.State);

                case "UET_Skybound":
                    if (!target.IsValid)
                        return false;

                    return ArmorTraitSkyboundUtility.TryJump(
                        caster,
                        target.Cell,
                        quality,
                        comp.State);

                case "UET_Shredding":
                    Pawn shreddingTarget = target.Thing as Pawn;

                    if (shreddingTarget == null)
                        return false;

                    return ArmorTraitShreddingUtility.TryApply(
                        caster,
                        shreddingTarget,
                        quality,
                        comp.State);

                case "UET_Overdrive":
                    return ArmorTraitOverdriveUtility.TryActivate(
                        caster,
                        quality,
                        comp.State);

                case "UET_Oppression":
                    Pawn oppressionTarget = target.Thing as Pawn;

                    if (oppressionTarget == null)
                        return false;

                    return ArmorTraitOppressionUtility.TryActivate(
                        caster,
                        oppressionTarget,
                        quality,
                        comp.State);

                case "UET_MindCleansing":
                    return ArmorTraitMindCleansingUtility.TryActivate(
                        caster,
                        quality,
                        comp.State);

                case "UET_Medic":
                    Pawn medicTarget = target.Thing as Pawn;

                    if (medicTarget == null)
                        return false;

                    return ArmorTraitMedicUtility.TryHeal(
                        caster,
                        medicTarget,
                        quality,
                        comp.State);

                case "UET_NeuralDeafness":
                    if (quality != QualityCategory.Legendary)
                        return false;

                    Pawn silenceTarget = target.Thing as Pawn;

                    if (silenceTarget == null)
                        return false;

                    return ArmorTraitSilenceUtility.TryActivate(
                        caster,
                        silenceTarget,
                        quality,
                        comp.State);

                case "UET_Kindle":
                    if (!target.IsValid)
                        return false;

                    return ArmorTraitKindleUtility.TryIgnite(
                        caster,
                        target.Cell,
                        quality,
                        comp.State);

                case "UET_Berserk":
                    Pawn berserkTarget = target.Thing as Pawn;

                    if (berserkTarget == null)
                        return false;

                    return ArmorTraitBerserkUtility.TryApply(
                        caster,
                        berserkTarget,
                        quality,
                        comp.State);

                default:
                    return false;
            }
        }

        // Compatibility with abilities that do not require a target.
        public static bool TryActivate(
            Pawn caster,
            Apparel apparel)
        {
            return TryActivate(
                caster,
                apparel,
                LocalTargetInfo.Invalid);
        }
    }
}

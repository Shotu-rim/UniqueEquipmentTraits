
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace UniqueEquipmentTraits.Core.Armor
{
    public partial class CompArmorTraits
    {
        // Armor buttons
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            if (traits == null || traits.Count == 0)
                yield break;

            QualityCategory quality;

            if (!parent.TryGetQuality(out quality))
                yield break;


            foreach (ArmorTraitDef trait in traits)
            {
                if (trait == null)
                    continue;

                if (trait.defName == "UET_Berserk")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo berserkGizmo =
                            ArmorTraitBerserkGizmo.Create(this, quality);

                        if (berserkGizmo != null)
                            yield return berserkGizmo;
                    }

                    continue;
                }

                if (trait.defName == "UET_Skybound")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo jumpGizmo =
                            ArmorTraitSkyboundGizmo.Create(this, quality);

                        if (jumpGizmo != null)
                            yield return jumpGizmo;
                    }

                    continue;
                }

                // Technomancer
                if (trait.defName == "UET_Technomancer")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo technomancerGizmo =
                            ArmorTraitTechnomancerGizmo.Create(
                                this,
                                quality);

                        if (technomancerGizmo != null)
                            yield return technomancerGizmo;
                    }

                    continue;
                }

                // Mind Cleansing
                if (trait.defName == "UET_MindCleansing")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo cleansingGizmo =
                            ArmorTraitMindCleansingGizmo.Create(this, quality);

                        if (cleansingGizmo != null)
                            yield return cleansingGizmo;
                    }

                    continue;
                }
                // Stunning Pulse
                if (trait.defName == "UET_StunningPulse")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo stunningPulseGizmo =
                            ArmorTraitStunningPulseGizmo.Create(
                                this,
                                quality);

                        if (stunningPulseGizmo != null)
                            yield return stunningPulseGizmo;
                    }

                    continue;
                }

                // Systemshock
                if (trait.defName == "UET_Systemshock")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo systemshockGizmo =
                            ArmorTraitSystemshockGizmo.Create(
                                this,
                                quality);

                        if (systemshockGizmo != null)
                            yield return systemshockGizmo;
                    }

                    continue;
                }

                // Overdrive
                if (trait.defName == "UET_Overdrive")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo overdriveGizmo =
                            ArmorTraitOverdriveGizmo.Create(
                                this,
                                quality);

                        if (overdriveGizmo != null)
                            yield return overdriveGizmo;
                    }

                    continue;
                }

                // Medic
                if (trait.defName == "UET_Medic")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo medicGizmo =
                            ArmorTraitMedicGizmo.Create(this, quality);

                        if (medicGizmo != null)
                            yield return medicGizmo;
                    }

                    continue;
                }
                // Shredding
                if (trait.defName == "UET_Shredding")
                {
                    Gizmo gizmo =
                        ArmorTraitShreddingGizmo.Create(
                            this,
                            quality);

                    if (gizmo != null)
                        yield return gizmo;
                }
                // Kindle

                if (trait.defName == "UET_Kindle")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo kindleGizmo =
                            ArmorTraitKindleGizmo.Create(this, quality);

                        if (kindleGizmo != null)
                            yield return kindleGizmo;
                    }

                    continue;
                }

                // Oppression
                if (trait.defName == "UET_Oppression")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo oppressionGizmo =
                            ArmorTraitOppressionGizmo.Create(
                                this,
                                quality);

                        if (oppressionGizmo != null)
                            yield return oppressionGizmo;
                    }

                    continue;
                }
                // Toxic Cloud
                if (trait.defName == "UET_ToxicCloud")
                {
                    if (quality == QualityCategory.Masterwork ||
                        quality == QualityCategory.Legendary)
                    {
                        Gizmo toxicCloudGizmo =
                            ArmorTraitToxicCloudGizmo.Create(
                                this,
                                quality);

                        if (toxicCloudGizmo != null)
                            yield return toxicCloudGizmo;
                    }

                    continue;
                }
                // Neural Deafness - Silence
                if (trait.defName == "UET_NeuralDeafness")
                {
                    if (quality == QualityCategory.Legendary)
                    {
                        Gizmo silenceGizmo =
                            ArmorTraitSilenceGizmo.Create(
                                this,
                                quality);

                        if (silenceGizmo != null)
                            yield return silenceGizmo;
                    }

                    continue;
                }

            }
        }
    }
}

UET — Mod Integration Quick Reference

1. Equipment & Crafting

UET automatically generates unique variants for eligible melee weapons and apparel, including equipment from other mods.

Equipment must support CompQuality.

Do not manually create duplicate UET_Unique_* variants.

Crafting integration uses GenRecipe.PostProcessProduct. Standard crafting recipes are processed automatically when an eligible variant exists.

Unique conversion is restricted to Masterwork and Legendary equipment.

UniqueCraftingChance.Calculate(skillLevel, quality, inspired) exposes the crafting probability.

Preserve the item's quality, material, ingredients and style when implementing custom crafting pipelines.

Do not bypass UET's variant registries when generating unique equipment.

2. Custom Weapon Traits

Define a RimWorld.WeaponTraitDef.

Add UniqueEquipmentTraits.Traits.UETTraitExtension in modExtensions.

Specify effectClass for custom effects.

Configure weaponCategory, canGenerateAlone and other Odyssey trait properties.

UET uses Odyssey's CompUniqueWeapon pipeline for trait assignment.

Respect the limit of one UET weapon trait per unique weapon.

3. Custom Armor Traits

Define UniqueEquipmentTraits.Core.Armor.ArmorTraitDef.

Set activationType to Passive, Reactive or Activated.

Configure allowedCategories, allowedStuffCategories, commonality, canGenerateAlone, legendaryOnly and exclusionTags as needed.

Set workerClass to a custom ArmorTraitWorker implementation when custom behavior is required.

Use ArmorTraitCompatibility.CanApply() before assigning traits.

Use CompArmorTraits.AddTrait() instead of modifying the trait list directly.

Each unique armor item supports one armor trait.

4. Charges, Cooldowns & Activation

Set <usesCharges>true</usesCharges> for charge-based armor traits.

Charge limits are determined by ArmorTraitChargeUtility.GetMaxCharges(). Custom per-trait capacities currently require C# integration.

Use ArmorTraitState.TryConsumeCharge() once per successful charge-consuming activation.

Check HasAvailableCharge, IsOnCooldown and IsEffectActive where applicable.

Cooldowns and active durations are measured in game ticks (60 ticks = 1 second).

ArmorTraitState stores charge regeneration, cooldowns and active-effect timing per equipment instance.

Use the shared activation pipeline for supported abilities. Avoid implementing independent cooldown or charge persistence unnecessarily.

For reactive traits, respect blocksReactivationWhileActive.

5. Optional DLC Compatibility

Odyssey is mandatory; Royalty, Biotech, Ideology and Anomaly are optional.

Guard DLC-specific code using ModsConfig.RoyaltyActive, BiotechActive, IdeologyActive or AnomalyActive.

Exclude unavailable DLC-dependent traits before generation or assignment.

Do not assume a DLC-specific Def, icon, stat or ability exists.

Use GetNamedSilentFail() and null checks for optional definitions.

Prefer checks during initialization, generation and activation rather than every tick.

UET's built-in DLC filtering is based on known trait defName values. Third-party traits need their own availability handling or explicit integration.

6. Compatibility Checklist

Odyssey dependency declared in About.xml.

Masterwork/Legendary restrictions respected.

Correct trait categories and equipment coverage.

No duplicate unique variants or conflicting components.

Charges consumed exactly once; cooldowns respected.

DLC-specific effects and assets safely guarded.

Tested with optional DLC enabled and disabled.

Save/load behavior tested for stateful traits.

Reference implementations: UniqueCraftingChance, ArmorTraitDef, ArmorTraitWorker, ArmorTraitCompatibility, ArmorTraitState, CompArmorTraits and UETTraitExtension.
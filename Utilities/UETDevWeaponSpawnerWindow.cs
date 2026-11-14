using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UniqueEquipmentTraits.Core;
using UniqueEquipmentTraits.Traits;
using UniqueEquipmentTraits.Utilities;
using UnityEngine;
using Verse;

namespace UniqueEquipmentTraits.DevTools
{
    internal sealed class UETDevWeaponSpawnerWindow : Window
    {
        private ThingDef selectedWeapon;
        private ThingDef selectedStuff;
        private QualityCategory selectedQuality =
            QualityCategory.Legendary;
        private RimWorld.WeaponTraitDef selectedTrait;

        private Vector2 weaponScroll;
        private string weaponSearch = "";

        private readonly List<ThingDef> weapons;

        public override Vector2 InitialSize =>
            new Vector2(650f, 650f);

        public UETDevWeaponSpawnerWindow()
        {
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;

            weapons = DefDatabase<ThingDef>.AllDefs
                .Where(d =>
                    d != null &&
                    d.defName != null &&
                    !d.defName.StartsWith("UET_Unique_", StringComparison.Ordinal) &&
                    d.IsWeapon &&
                    d.HasComp<CompQuality>() &&
                    (d.IsMeleeWeapon ||
                     d.IsRangedWeapon) &&
                    (d.HasComp<CompUniqueWeapon>() ||
                     UniqueWeaponVariantRegistry
                         .TryGetVariant(d, out _)))
                .OrderBy(d => d.label)
                .ToList();

            if (weapons.Count > 0)
                SelectWeapon(weapons[0]);
        }

        private void SelectWeapon(ThingDef weapon)
        {
            selectedWeapon = weapon;
            selectedStuff = null;
            selectedTrait = null;

            if (weapon.MadeFromStuff)
            {
                selectedStuff =
                    GenStuff.DefaultStuffFor(weapon);
            }

            List<RimWorld.WeaponTraitDef> traits =
                GetCompatibleTraits();

            if (traits.Count > 0)
                selectedTrait = traits[0];
        }

        private List<RimWorld.WeaponTraitDef>
            GetCompatibleTraits()
        {
            if (selectedWeapon == null)
                return new List<RimWorld.WeaponTraitDef>();

            ThingDef spawnDef = selectedWeapon;

            if (UniqueWeaponVariantRegistry.TryGetVariant(
                    selectedWeapon, out ThingDef variant))
            {
                spawnDef = variant;
            }

            CompProperties_UniqueWeapon props =
                spawnDef.comps?
                    .OfType<CompProperties_UniqueWeapon>()
                    .FirstOrDefault();

            if (props?.weaponCategories == null)
                return new List<RimWorld.WeaponTraitDef>();

            return DefDatabase<RimWorld.WeaponTraitDef>
                .AllDefs
                .Where(t =>
                    t.GetModExtension<UETTraitExtension>()
                        != null &&
                    t.canGenerateAlone &&
                    t.weaponCategory != null &&
                    props.weaponCategories.Contains(
                        t.weaponCategory))
                .OrderBy(t => t.label)
                .ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!Prefs.DevMode)
            {
                Close();
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(
                new Rect(0f, 0f, inRect.width, 35f),
                "UET Dev Weapon Spawner");

            Text.Font = GameFont.Small;

            float y = 45f;

            Widgets.Label(
                new Rect(0f, y, 150f, 28f),
                "Search weapon:");

            weaponSearch = Widgets.TextField(
                new Rect(150f, y, inRect.width - 150f, 28f),
                weaponSearch);

            y += 38f;

            Rect listRect = new Rect(
                0f, y, inRect.width, 235f);

            DrawWeaponList(listRect);

            y += 245f;

            DrawSelectionControls(inRect, ref y);
        }

        private void DrawWeaponList(Rect rect)
        {
            List<ThingDef> filtered = weapons
                .Where(d =>
                    string.IsNullOrEmpty(weaponSearch) ||
                    d.label.IndexOf(
                        weaponSearch,
                        StringComparison.OrdinalIgnoreCase)
                        >= 0 ||
                    d.defName.IndexOf(
                        weaponSearch,
                        StringComparison.OrdinalIgnoreCase)
                        >= 0)
                .ToList();

            Rect viewRect = new Rect(
                0f, 0f,
                rect.width - 18f,
                filtered.Count * 30f);

            Widgets.BeginScrollView(
                rect, ref weaponScroll, viewRect);

            for (int i = 0; i < filtered.Count; i++)
            {
                ThingDef weapon = filtered[i];

                Rect row = new Rect(
                    0f, i * 30f,
                    viewRect.width, 28f);

                if (weapon == selectedWeapon)
                    Widgets.DrawHighlightSelected(row);
                else
                    Widgets.DrawHighlightIfMouseover(row);

                if (Widgets.ButtonInvisible(row))
                    SelectWeapon(weapon);

                Widgets.Label(
                    row.ContractedBy(3f),
                    weapon.LabelCap);
            }

            Widgets.EndScrollView();
        }

        private void DrawSelectionControls(
            Rect rect, ref float y)
        {
            if (selectedWeapon == null)
            {
                Widgets.Label(
                    new Rect(0f, y, rect.width, 30f),
                    "No compatible weapons found.");
                return;
            }

            Widgets.Label(
                new Rect(0f, y, rect.width, 28f),
                "Weapon: " + selectedWeapon.LabelCap);

            y += 34f;

            DrawStuffButton(rect, ref y);
            DrawQualityButton(rect, ref y);
            DrawTraitButton(rect, ref y);

            y += 12f;

            if (Widgets.ButtonText(
                new Rect(0f, y, rect.width, 35f),
                "Create weapon"))
            {
                CreateWeapon();
            }
        }

        private void DrawStuffButton(
            Rect rect, ref float y)
        {
            if (!selectedWeapon.MadeFromStuff)
            {
                Widgets.Label(
                    new Rect(0f, y, rect.width, 28f),
                    "Material: Not applicable");

                y += 38f;
                return;
            }

            string label = selectedStuff != null
                ? selectedStuff.LabelCap.ToString()
                : "Select material";

            if (Widgets.ButtonText(
                new Rect(0f, y, rect.width, 30f),
                "Material: " + label))
            {
                List<FloatMenuOption> options =
                    DefDatabase<ThingDef>.AllDefs
                        .Where(d =>
                            d.IsStuff &&
                            d.stuffProps != null &&
                            selectedWeapon.stuffCategories
                                .Any(c =>
                                    d.stuffProps.categories
                                        .Contains(c)))
                        .OrderBy(d => d.label)
                        .Select(d =>
                            new FloatMenuOption(
                                d.LabelCap.ToString(),
                                () => selectedStuff = d))
                        .ToList();

                Find.WindowStack.Add(
                    new FloatMenu(options));
            }

            y += 38f;
        }

        private void DrawQualityButton(
            Rect rect, ref float y)
        {
            if (Widgets.ButtonText(
                new Rect(0f, y, rect.width, 30f),
                "Quality: " + selectedQuality))
            {
                Find.WindowStack.Add(
                    new FloatMenu(
                        new List<FloatMenuOption>
                        {
                            new FloatMenuOption(
                                "Masterwork",
                                () => selectedQuality =
                                    QualityCategory.Masterwork),

                            new FloatMenuOption(
                                "Legendary",
                                () => selectedQuality =
                                    QualityCategory.Legendary)
                        }));
            }

            y += 38f;
        }

        private void DrawTraitButton(
            Rect rect, ref float y)
        {
            List<RimWorld.WeaponTraitDef> traits =
                GetCompatibleTraits();

            string label = selectedTrait != null
                ? selectedTrait.LabelCap.ToString()
                : "No compatible trait";

            if (Widgets.ButtonText(
                new Rect(0f, y, rect.width, 30f),
                "Trait: " + label))
            {
                List<FloatMenuOption> options =
                    traits.Select(t =>
                        new FloatMenuOption(
                            t.LabelCap.ToString(),
                            () => selectedTrait = t))
                        .ToList();

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(
                        new FloatMenu(options));
                }
            }

            y += 38f;
        }

        private void CreateWeapon()
        {
            if (selectedWeapon == null ||
                selectedTrait == null)
                return;

            ThingWithComps weapon =
                UETDevWeaponSpawner.Create(
                    selectedWeapon,
                    selectedStuff,
                    selectedTrait,
                    selectedQuality);

            if (weapon == null)
            {
                Messages.Message(
                    "[UET] Weapon generation failed.",
                    MessageTypeDefOf.RejectInput,
                    false);
                return;
            }

            Map map = Find.CurrentMap;

            if (map == null)
            {
                weapon.Destroy();
                return;
            }

            IntVec3 cell =
                UI.MouseCell();

            if (!cell.InBounds(map) ||
                !cell.Standable(map))
            {
                cell = map.Center;
            }

            GenPlace.TryPlaceThing(
                weapon,
                cell,
                map,
                ThingPlaceMode.Near);

            Messages.Message(
                "[UET] Created " + weapon.LabelCap,
                MessageTypeDefOf.PositiveEvent,
                false);
        }
    }
}

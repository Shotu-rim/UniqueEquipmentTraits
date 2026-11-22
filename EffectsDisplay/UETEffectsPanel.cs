
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using UniqueEquipmentTraits.Core.Armor;

namespace UniqueEquipmentTraits.EffectsDisplay
{
    /// <summary>
    /// Draws equipment effects in the pawn Health tab.
    /// Shows trait status, charges and equipment details.
    /// </summary>
    public static class UETEffectsPanel
    {
        private const float Padding = 12f;
        private const float HeaderHeight = 30f;
        private const float RowHeight = 48f;
        private const float RowSpacing = 6f;

        private static Vector2 scrollPosition = Vector2.zero;

        public static void Draw(Rect rect, Pawn pawn)
        {
            if (pawn == null)
                return;

            Widgets.DrawLineVertical(
                rect.x, rect.y, rect.height);

            Rect contentRect = rect.ContractedBy(Padding);

            Rect headerRect = new Rect(
                contentRect.x,
                contentRect.y,
                contentRect.width,
                HeaderHeight);

            Widgets.Label(headerRect, "Effects");

            Rect scrollRect = new Rect(
                contentRect.x,
                headerRect.yMax + 4f,
                contentRect.width,
                contentRect.height - HeaderHeight - 4f);

            if (scrollRect.height <= 0f)
                return;

            List<UETEffectDisplayEntry> effects =
                UETEffectDisplayManager.GetEffects(pawn);

            float contentHeight =
                effects.Count * (RowHeight + RowSpacing);

            Rect viewRect = new Rect(
                0f,
                0f,
                scrollRect.width - 16f,
                Mathf.Max(scrollRect.height, contentHeight));

            Widgets.BeginScrollView(
                scrollRect,
                ref scrollPosition,
                viewRect);

            float y = 0f;

            foreach (UETEffectDisplayEntry effect in effects)
            {
                Rect rowRect = new Rect(
                    0f, y, viewRect.width, RowHeight);

                DrawEffectRow(rowRect, pawn, effect);

                y += RowHeight + RowSpacing;
            }

            Widgets.EndScrollView();
        }

        private static void DrawEffectRow(
            Rect rect,
            Pawn pawn,
            UETEffectDisplayEntry effect)
        {
            if (effect == null)
                return;

            Widgets.DrawHighlightIfMouseover(rect);

            Hediff hediff = effect.Hediff;

            if (hediff != null)
            {
                DrawHediffRow(rect, pawn, effect);
                return;
            }

            // First line: trait name and charge status
            string chargesText = GetChargesText(effect);

            float chargesWidth = string.IsNullOrEmpty(chargesText)
                ? 0f
                : Mathf.Min(
                    Text.CalcSize(chargesText).x + 8f,
                    rect.width * 0.55f);

            Rect nameRect = new Rect(
                rect.x,
                rect.y,
                Mathf.Max(0f, rect.width - chargesWidth - 4f),
                24f);

            Rect chargesRect = new Rect(
                rect.xMax - chargesWidth,
                rect.y,
                chargesWidth,
                24f);

            Color previousColor = GUI.color;

            if (effect.IsActive)
                GUI.color = Color.green;

            Widgets.Label(nameRect, effect.Label);

            GUI.color = previousColor;

            if (!string.IsNullOrEmpty(chargesText))
            {
                TextAnchor previousAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;

                Widgets.Label(chargesRect, chargesText);

                Text.Anchor = previousAnchor;
            }

            // Second line: last triggered body part and status
            Rect bodyPartRect = new Rect(
                rect.x,
                rect.y + 24f,
                rect.width * 0.4f,
                22f);

            Rect statusRect = new Rect(
                rect.x + rect.width * 0.4f,
                rect.y + 24f,
                rect.width * 0.6f,
                22f);

            bool active = effect.RemainingActiveTicks > 0;
            bool coolingDown =
                !active && effect.RemainingCooldownTicks > 0;

            if ((active || coolingDown) &&
                !string.IsNullOrEmpty(effect.BodyPartLabel))
            {
                Widgets.Label(
                    bodyPartRect,
                    effect.BodyPartLabel.CapitalizeFirst());
            }

            string status;

            if (effect.IsPassive)
            {
                status = "Active";
            }
            else if (active)
            {
                status = "Active: " +
                    FormatRealSeconds(effect.RemainingActiveTicks);
            }
            else if (coolingDown)
            {
                status = "Cooldown: " +
                    (effect.EffectId == "UET_MindCleansing"
                        ? FormatGameDuration(effect.RemainingCooldownTicks)
                        : FormatRealSeconds(effect.RemainingCooldownTicks));
            }
            else if (effect.MaxCharges > 0 &&
                     effect.Charges <= 0)
            {
                status = "No charges";
            }
            else
            {
                status = "Ready to activate";
            }

            TextAnchor oldAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;

            Widgets.Label(statusRect, status);

            Text.Anchor = oldAnchor;

            TooltipHandler.TipRegion(
                rect,
                () => BuildEffectTooltip(effect, pawn),
                GetEffectTooltipId(effect));
        }

        private static void DrawHediffRow(
     Rect rect,
     Pawn pawn,
     UETEffectDisplayEntry effect)
        {
            Hediff hediff = effect.Hediff;

            if (hediff == null)
                return;

            string chargesText = GetChargesText(effect);

            float chargesWidth = string.IsNullOrEmpty(chargesText)
                ? 0f
                : Mathf.Min(
                    Text.CalcSize(chargesText).x + 8f,
                    rect.width * 0.55f);

            Rect labelRect = new Rect(
                rect.x,
                rect.y,
                Mathf.Max(0f, rect.width - chargesWidth - 4f),
                rect.height);

            Rect chargesRect = new Rect(
                rect.xMax - chargesWidth,
                rect.y,
                chargesWidth,
                rect.height);

            Color previousColor = GUI.color;
            GUI.color = hediff.LabelColor;

            Widgets.Label(labelRect, hediff.LabelCap);

            GUI.color = previousColor;

            if (!string.IsNullOrEmpty(chargesText))
            {
                TextAnchor previousAnchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleRight;

                Widgets.Label(chargesRect, chargesText);

                Text.Anchor = previousAnchor;
            }

            TooltipHandler.TipRegion(
                rect,
                () => hediff.GetTooltip(pawn, false),
                hediff.GetHashCode());
        }

        private static string GetChargesText(
            UETEffectDisplayEntry effect)
        {
            if (effect.MaxCharges <= 0)
                return string.Empty;

            string text =
                effect.Charges + "/" + effect.MaxCharges;

            if (effect.Charges < effect.MaxCharges &&
                effect.RemainingChargeTicks > 0)
            {
                text += " (" +
                    FormatGameDuration(effect.RemainingChargeTicks) +
                    ")";
            }

            return text;
        }

        private static string BuildEffectTooltip(
            UETEffectDisplayEntry effect,
            Pawn pawn)
        {
            StringBuilder tooltip = new StringBuilder();

            if (!string.IsNullOrEmpty(effect.Description))
            {
                tooltip.Append(effect.Description.Trim());
            }

            Apparel apparel = effect.Source as Apparel;

            if (apparel != null)
            {
                if (tooltip.Length > 0)
                    tooltip.AppendLine().AppendLine();

                tooltip.AppendLine("Equipment:");
                tooltip.AppendLine(apparel.LabelCap);

                BodyDef body = pawn?.RaceProps?.body;

                if (body != null)
                {
                    List<ArmorTraitCategory> categories =
                        ArmorTraitCoverage.GetCoveredCategories(
                            apparel.def, body);

                    if (categories.Count > 0)
                    {
                        tooltip.AppendLine();
                        tooltip.AppendLine("Protected body parts:");

                        List<string> labels = new List<string>();

                        foreach (ArmorTraitCategory category
                            in categories)
                        {
                            string label =
                                GetCategoryDisplayName(category);

                            if (!string.IsNullOrEmpty(label) &&
                                !labels.Contains(label))
                            {
                                labels.Add(label);
                            }
                        }

                        tooltip.Append(
                            string.Join(", ", labels.ToArray()));
                    }
                }
            }

            return tooltip.ToString();
        }

        private static string GetCategoryDisplayName(
            ArmorTraitCategory category)
        {
            switch (category)
            {
                case ArmorTraitCategory.Helmet:
                    return "Head";

                case ArmorTraitCategory.Neck:
                    return "Neck";

                case ArmorTraitCategory.Shoulders:
                    return "Shoulders";

                case ArmorTraitCategory.Torso:
                    return "Torso";

                case ArmorTraitCategory.Arms:
                    return "Arms";

                case ArmorTraitCategory.Hands:
                    return "Hands";

                case ArmorTraitCategory.Waist:
                    return "Waist";

                case ArmorTraitCategory.Legs:
                    return "Legs";

                case ArmorTraitCategory.Feet:
                    return "Feet";

                default:
                    return null;
            }
        }

        private static int GetEffectTooltipId(
            UETEffectDisplayEntry effect)
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 31 +
                    (effect.Source != null
                        ? effect.Source.thingIDNumber
                        : 0);

                hash = hash * 31 +
                    (effect.EffectId != null
                        ? effect.EffectId.GetHashCode()
                        : 0);

                return hash;
            }
        }

        private static string FormatRealSeconds(int ticks)
        {
            if (ticks <= 0)
                return "0.0 s";

            return (ticks / 60f).ToString("0.0") + " s";
        }

        private static string FormatGameDuration(int ticks)
        {
            if (ticks <= 0)
                return "0 m";

            const int TicksPerHour = 2500;
            const float TicksPerMinute = TicksPerHour / 60f;

            if (ticks >= TicksPerHour)
            {
                float hours = ticks / (float)TicksPerHour;
                return hours.ToString("0.#") + " h";
            }

            int minutes = Mathf.CeilToInt(
                ticks / TicksPerMinute);

            return minutes + " m";
        }
    }
}

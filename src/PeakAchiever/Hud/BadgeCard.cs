using System.Collections.Generic;
using PeakAchiever.Game;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Hud;

/// <summary>One tracked badge on the HUD: icon with its state mark, name, condition and status row.</summary>
internal sealed class BadgeCard
{
    // Sizes in reference pixels of the 1920x1080 canvas, taken from the signed-off mockup.
    public const float Width = 340f;
    private const float IconSize = 52f;
    private const float MarkSize = 24f;
    private const float MarkRingWidth = 2f;
    private const float MarkGlyphSize = 14f;
    private const float BarHeight = 8f;
    private const float NameFontSize = 17f;
    private const float DescriptionFontSize = 13f;
    private const float StatusFontSize = 12f;
    private const int Padding = 10;
    private const float Gap = 10f;
    private const float StatusRowGap = 8f;
    private const float ChecklistIconSize = 26f;
    private const float ChecklistGap = 4f;
    private const float ChecklistTickSize = 13f;
    private const float ChecklistTickGlyphSize = 9f;
    // As many icons as fit the text column: (Width - 2 * Padding - IconSize - Gap + ChecklistGap) / (ChecklistIconSize + ChecklistGap).
    private const int ChecklistColumns = 8;
    private static readonly string CurrentSplitColor = ColorUtility.ToHtmlStringRGB(HudStyle.ProgressFill);

    private readonly HudStyle _style;
    private readonly RawImage _icon;
    private readonly Image _markRing;
    private readonly Image _markGlyph;
    private readonly TextMeshProUGUI _name;
    private readonly TextMeshProUGUI _description;
    private readonly GameObject _bar;
    private readonly RectTransform _barFill;
    private readonly Image _barFillImage;
    private readonly TextMeshProUGUI _count;
    private readonly TextMeshProUGUI _status;
    private readonly TextMeshProUGUI _detail;
    private readonly GameObject _checklist;
    private readonly List<ChecklistCell> _checklistCells = [];

    public BadgeCard(Transform parent, HudStyle style)
    {
        _style = style;
        Root = UiFactory.Create("BadgeCard", parent);
        UiFactory.AddImage(Root, style.RoundedRect, HudStyle.CardBackground).type = Image.Type.Sliced;
        HorizontalLayoutGroup row = Root.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(Padding, Padding, Padding, Padding);
        row.spacing = Gap;
        row.childAlignment = TextAnchor.UpperLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;
        UiFactory.SetPreferredSize(Root, Width, -1f);

        GameObject iconSlot = UiFactory.Create("Icon", Root.transform);
        UiFactory.SetFixedSize(iconSlot, IconSize, IconSize);
        _icon = iconSlot.AddComponent<RawImage>();

        GameObject mark = UiFactory.Create("Mark", iconSlot.transform);
        UiFactory.PinToCorner(mark, new Vector2(1f, 0f), new Vector2(4f, -4f), MarkSize);
        _markRing = UiFactory.AddImage(mark, style.Circle, Color.white);
        GameObject markInside = UiFactory.Create("Inside", mark.transform);
        UiFactory.PinToCorner(markInside, new Vector2(0.5f, 0.5f), Vector2.zero, MarkSize - 2 * MarkRingWidth);
        UiFactory.AddImage(markInside, style.Circle, HudStyle.MarkBackground);
        GameObject glyph = UiFactory.Create("Glyph", mark.transform);
        UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, MarkGlyphSize);
        _markGlyph = UiFactory.AddImage(glyph, style.Check, Color.white);
        _markGlyph.preserveAspect = true;

        GameObject column = UiFactory.Create("Text", Root.transform);
        VerticalLayoutGroup stack = column.AddComponent<VerticalLayoutGroup>();
        stack.spacing = 3f;
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;
        column.AddComponent<LayoutElement>().flexibleWidth = 1f;

        _name = UiFactory.AddText(column.transform, "Name", style.DisplayFont, NameFontSize, HudStyle.Ink);
        _description = UiFactory.AddText(column.transform, "Description", style.BodyFont, DescriptionFontSize, HudStyle.InkSoft);

        GameObject statusRow = UiFactory.Create("Status", column.transform);
        HorizontalLayoutGroup statusLayout = statusRow.AddComponent<HorizontalLayoutGroup>();
        statusLayout.spacing = StatusRowGap;
        statusLayout.childAlignment = TextAnchor.MiddleLeft;
        statusLayout.childControlWidth = true;
        statusLayout.childControlHeight = true;
        statusLayout.childForceExpandWidth = false;
        statusLayout.childForceExpandHeight = false;

        _bar = UiFactory.Create("Bar", statusRow.transform);
        UiFactory.AddImage(_bar, style.RoundedRect, HudStyle.BarTrack).type = Image.Type.Sliced;
        LayoutElement barLayout = _bar.AddComponent<LayoutElement>();
        barLayout.flexibleWidth = 1f;
        barLayout.preferredHeight = BarHeight;
        GameObject fill = UiFactory.Create("Fill", _bar.transform);
        _barFill = (RectTransform)fill.transform;
        _barFill.anchorMin = Vector2.zero;
        _barFill.offsetMin = Vector2.zero;
        _barFill.offsetMax = Vector2.zero;
        _barFillImage = UiFactory.AddImage(fill, style.RoundedRect, HudStyle.ProgressFill);
        _barFillImage.type = Image.Type.Sliced;

        _count = UiFactory.AddText(statusRow.transform, "Count", style.StrongFont, StatusFontSize, HudStyle.Ink);
        _status = UiFactory.AddText(statusRow.transform, "Label", style.StrongFont, StatusFontSize, HudStyle.ProgressFill);
        _detail = UiFactory.AddText(column.transform, "Detail", style.BodyFont, StatusFontSize, HudStyle.InkSoft);

        _checklist = UiFactory.Create("Checklist", column.transform);
        GridLayoutGroup grid = _checklist.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(ChecklistIconSize, ChecklistIconSize);
        grid.spacing = new Vector2(ChecklistGap, ChecklistGap);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = ChecklistColumns;
    }

    public GameObject Root { get; }

    public void Show(BadgePresentation badge, TrackedStatus status, BadgeDetail? detail)
    {
        _icon.texture = badge.Icon;
        bool unattainable = status is TrackedStatus.Unattainable;
        _icon.color = badge.IsHidden || unattainable ? HudStyle.LockedIconTint : Color.white;
        _name.text = unattainable ? $"<s>{badge.Name}</s>" : badge.Name;
        _name.color = unattainable ? HudStyle.InkMuted : HudStyle.Ink;
        _description.text = badge.Description;
        _description.color = unattainable ? HudStyle.InkMuted : HudStyle.InkSoft;

        switch (status)
        {
            case TrackedStatus.Attainable attainable:
                ShowMark(null, default);
                ShowProgress(attainable.Progress, HudStyle.ProgressFill);
                if (attainable.Progress is { } progress)
                    ShowLabel(ScopeLabel(progress), HudStyle.InkMuted);
                else
                    ShowLabel(ModText.Get(ModTextKey.StatusAttainable), HudStyle.ProgressFill);
                break;
            case TrackedStatus.Holding holding:
                ShowMark(null, default);
                ShowProgress(holding.Progress, HudStyle.ProgressFill);
                // A measured limit already says how the condition holds.
                ShowLabel(holding.Progress is null ? ModText.Get(ModTextKey.StatusHolding) : "", HudStyle.ProgressFill);
                break;
            case TrackedStatus.Achieved achieved:
                ShowMark(_style.Check, HudStyle.Achieved);
                ShowProgress(achieved.Progress, HudStyle.Achieved);
                ShowLabel(ModText.Get(ModTextKey.StatusAchieved), HudStyle.Achieved);
                break;
            case TrackedStatus.Unattainable blocked:
                ShowMark(_style.Cross, HudStyle.Unattainable);
                ShowProgress(null, default);
                ShowLabel(StatusText.Describe(blocked.Reason), HudStyle.Unattainable);
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(status), status, "Unhandled tracked status.");
        }
        ShowDetail(detail, unattainable);
    }

    private static string ScopeLabel(Progress progress) =>
        progress.Scope == ProgressScope.Lifetime ? ModText.Get(ModTextKey.ScopeLifetime) : "";

    private void ShowMark(Sprite? glyph, Color color)
    {
        _markRing.gameObject.SetActive(glyph != null);
        if (glyph == null)
            return;
        _markGlyph.sprite = glyph;
        _markGlyph.color = color;
        _markRing.color = color;
    }

    private void ShowProgress(Progress? progress, Color fillColor)
    {
        _bar.SetActive(progress != null);
        _count.gameObject.SetActive(progress != null);
        if (progress is not { } shown)
            return;
        _barFill.anchorMax = new Vector2(shown.Fraction, 1f);
        _barFillImage.color = fillColor;
        _count.text = StatusText.Count(shown);
    }

    private void ShowDetail(BadgeDetail? detail, bool unattainable)
    {
        string text = "";
        IReadOnlyList<ChecklistItem> items = [];
        switch (detail)
        {
            case null:
                break;
            // Once broken the bar is gone, so the elapsed time moves down here.
            case BadgeDetail.RunClock clock:
                text = StatusText.RunClock(clock, withElapsed: unattainable, CurrentSplitColor);
                break;
            case BadgeDetail.Checklist checklist:
                items = checklist.Items;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(detail), detail, "Unhandled badge detail.");
        }
        _detail.text = text;
        // The timeline stays empty for the first seconds of a run.
        _detail.gameObject.SetActive(text.Length > 0);
        ShowChecklist(items);
    }

    private void ShowChecklist(IReadOnlyList<ChecklistItem> items)
    {
        _checklist.SetActive(items.Count > 0);
        while (_checklistCells.Count < items.Count)
            _checklistCells.Add(new ChecklistCell(_checklist.transform, _style));
        for (int i = 0; i < _checklistCells.Count; i++)
        {
            bool used = i < items.Count;
            _checklistCells[i].Root.SetActive(used);
            if (used)
                _checklistCells[i].Show(items[i]);
        }
    }

    private void ShowLabel(string text, Color color)
    {
        _status.gameObject.SetActive(text.Length > 0);
        _status.text = text;
        _status.color = color;
    }

    /// <summary>One item of a checklist: its icon, dimmed until eaten, then full colour with a green tick.</summary>
    private sealed class ChecklistCell
    {
        private readonly RawImage _icon;
        private readonly GameObject _tick;

        public ChecklistCell(Transform parent, HudStyle style)
        {
            Root = UiFactory.Create("Item", parent);
            _icon = Root.AddComponent<RawImage>();
            _tick = UiFactory.Create("Tick", Root.transform);
            UiFactory.PinToCorner(_tick, new Vector2(1f, 0f), new Vector2(2f, -2f), ChecklistTickSize);
            UiFactory.AddImage(_tick, style.Circle, HudStyle.MarkBackground);
            GameObject glyph = UiFactory.Create("Glyph", _tick.transform);
            UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, ChecklistTickGlyphSize);
            UiFactory.AddImage(glyph, style.Check, HudStyle.Achieved).preserveAspect = true;
        }

        public GameObject Root { get; }

        public void Show(ChecklistItem item)
        {
            _icon.texture = ItemCatalog.Icon(item.ItemId);
            _icon.color = item.Eaten ? Color.white : HudStyle.LockedIconTint;
            _tick.SetActive(item.Eaten);
        }
    }
}

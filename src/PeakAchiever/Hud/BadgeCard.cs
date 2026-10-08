using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Game;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Hud;

/// <summary>
/// One tracked badge on the HUD, a stitched card: the icon with its state mark beside the name, its chips,
/// the condition and the status row; under them, across the whole card, what the badge needs. Folded to one
/// line once earned or torn.
/// </summary>
internal sealed class BadgeCard
{
    // Sizes in reference pixels of the 1920x1080 canvas, taken from the signed-off mockup.
    public const float Width = 340f;
    private const float IconSize = 52f;
    private const float CompactIconSize = 30f;
    private const float MarkSize = 24f;
    private const float CompactMarkSize = 16f;
    private const float MarkRingWidth = 2f;
    private const float MarkGlyphSize = 14f;
    private const float BarHeight = 8f;
    private const float BarRimWidth = 1f;
    // Keeps the bar readable when the figures beside it are long.
    private const float MinBarWidth = 40f;
    private const float NameFontSize = 17f;
    private const float DescriptionFontSize = 13f;
    private const float StatusFontSize = 12f;
    private const float ChipFontSize = 9.5f;
    private const float ChipHeight = 15f;
    private const float ChipRimWidth = 1f;
    private const int ChipPaddingX = 6;
    private const float ChipGap = 4f;
    private const int Padding = 13;
    private const int CompactPaddingY = 6;
    // The seam runs this far inside the card's edge.
    private const float SeamInset = 5f;
    private const float Gap = 10f;
    private const float SectionGap = 7f;
    private const float TextGap = 3f;
    private const float StatusRowGap = 8f;
    private const float ChecklistIconSize = 26f;
    private const float ChecklistGap = 4f;
    private const float ChecklistTickSize = 13f;
    private const float ChecklistTickGlyphSize = 9f;
    // As many icons as fit the text column: (Width - 2 * Padding - IconSize - Gap + ChecklistGap) / (ChecklistIconSize + ChecklistGap).
    private const int ChecklistColumns = 8;
    private static readonly Color NotOnMapTint = new(HudStyle.LockedIconTint.r, HudStyle.LockedIconTint.g, HudStyle.LockedIconTint.b, 0.4f);
    private static readonly ClockColors ClockColors = new(
        Current: ColorUtility.ToHtmlStringRGB(HudStyle.ProgressFill),
        Slower: ColorUtility.ToHtmlStringRGB(HudStyle.Unattainable),
        Faster: ColorUtility.ToHtmlStringRGB(HudStyle.Achieved),
        Caution: ColorUtility.ToHtmlStringRGB(HudStyle.Caution)
    );

    private readonly HudStyle _style;
    private readonly VerticalLayoutGroup _layout;
    private readonly LayoutElement _iconSlot;
    private readonly RawImage _icon;
    private readonly RectTransform _mark;
    private readonly Image _markRing;
    private readonly Image _markGlyph;
    private readonly TextMeshProUGUI _name;
    private readonly GameObject _chips;
    private readonly Chip _scopeChip;
    private readonly Chip _allyChip;
    private readonly TextMeshProUGUI _description;
    private readonly GameObject _statusRow;
    private readonly GameObject _bar;
    private readonly AfflictionBar _afflictionBar;
    private readonly RectTransform _barFill;
    private readonly Image _barFillImage;
    private readonly TextMeshProUGUI _count;
    private readonly TextMeshProUGUI _status;
    private readonly TextMeshProUGUI _detail;
    private readonly ChecklistGrid _onMap;
    private readonly TextMeshProUGUI _notOnMapLabel;
    private readonly ChecklistGrid _notOnMap;

    public BadgeCard(Transform parent, HudStyle style)
    {
        _style = style;
        Root = UiFactory.Create("BadgeCard", parent);
        UiFactory.AddImage(Root, style.RoundedRect, HudStyle.CardBackground).type = Image.Type.Sliced;
        AddSeam(Root.transform, style);
        _layout = Root.AddComponent<VerticalLayoutGroup>();
        _layout.padding = new RectOffset(Padding, Padding, Padding, Padding);
        _layout.spacing = SectionGap;
        _layout.childControlWidth = true;
        _layout.childControlHeight = true;
        _layout.childForceExpandWidth = true;
        _layout.childForceExpandHeight = false;
        UiFactory.SetPreferredSize(Root, Width, -1f);

        GameObject top = UiFactory.Create("Top", Root.transform);
        HorizontalLayoutGroup row = top.AddComponent<HorizontalLayoutGroup>();
        row.spacing = Gap;
        row.childAlignment = TextAnchor.UpperLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;

        GameObject iconColumn = UiFactory.Create("IconColumn", top.transform);
        VerticalLayoutGroup iconStack = iconColumn.AddComponent<VerticalLayoutGroup>();
        iconStack.spacing = Gap;
        iconStack.childAlignment = TextAnchor.UpperCenter;
        iconStack.childControlWidth = true;
        iconStack.childControlHeight = true;
        iconStack.childForceExpandWidth = false;
        iconStack.childForceExpandHeight = false;
        IconColumn = iconColumn.transform;

        GameObject iconSlot = UiFactory.Create("Icon", iconColumn.transform);
        UiFactory.SetFixedSize(iconSlot, IconSize, IconSize);
        _iconSlot = iconSlot.GetComponent<LayoutElement>();
        _icon = iconSlot.AddComponent<RawImage>();

        GameObject mark = UiFactory.Create("Mark", iconSlot.transform);
        UiFactory.PinToCorner(mark, new Vector2(1f, 0f), new Vector2(4f, -4f), MarkSize);
        _mark = (RectTransform)mark.transform;
        _markRing = UiFactory.AddImage(mark, style.Circle, Color.white);
        GameObject markInside = UiFactory.Create("Inside", mark.transform);
        Fill((RectTransform)markInside.transform, MarkRingWidth);
        UiFactory.AddImage(markInside, style.Circle, HudStyle.MarkBackground);
        GameObject glyph = UiFactory.Create("Glyph", mark.transform);
        Fill((RectTransform)glyph.transform, (MarkSize - MarkGlyphSize) / 2f);
        _markGlyph = UiFactory.AddImage(glyph, style.Check, Color.white);
        _markGlyph.preserveAspect = true;

        GameObject column = UiFactory.Create("Text", top.transform);
        VerticalLayoutGroup stack = column.AddComponent<VerticalLayoutGroup>();
        stack.spacing = TextGap;
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;
        column.AddComponent<LayoutElement>().flexibleWidth = 1f;

        _name = UiFactory.AddText(column.transform, "Name", style.DisplayFont, NameFontSize, HudStyle.Ink);
        _chips = UiFactory.Create("Chips", column.transform);
        HorizontalLayoutGroup chipRow = _chips.AddComponent<HorizontalLayoutGroup>();
        chipRow.spacing = ChipGap;
        chipRow.childAlignment = TextAnchor.MiddleLeft;
        chipRow.childControlWidth = true;
        chipRow.childControlHeight = true;
        chipRow.childForceExpandWidth = false;
        chipRow.childForceExpandHeight = false;
        _scopeChip = new Chip(_chips.transform, style, ModText.Get(ModTextKey.ChipAllRuns), HudStyle.InkMuted);
        _allyChip = new Chip(_chips.transform, style, ModText.Get(ModTextKey.ChipForAlly), HudStyle.InkSoft);
        _description = UiFactory.AddText(column.transform, "Description", style.BodyFont, DescriptionFontSize, HudStyle.InkSoft);

        _statusRow = UiFactory.Create("Status", column.transform);
        HorizontalLayoutGroup statusLayout = _statusRow.AddComponent<HorizontalLayoutGroup>();
        statusLayout.spacing = StatusRowGap;
        statusLayout.childAlignment = TextAnchor.MiddleLeft;
        statusLayout.childControlWidth = true;
        statusLayout.childControlHeight = true;
        statusLayout.childForceExpandWidth = false;
        statusLayout.childForceExpandHeight = false;

        // The rim, then the track inset by the rim's width, then the fill inside the track.
        _bar = UiFactory.Create("Bar", _statusRow.transform);
        UiFactory.AddPill(_bar, style, HudStyle.BarRim, BarHeight);
        LayoutElement barLayout = _bar.AddComponent<LayoutElement>();
        barLayout.flexibleWidth = 1f;
        barLayout.minWidth = MinBarWidth;
        barLayout.preferredHeight = BarHeight;
        float trackHeight = BarHeight - 2 * BarRimWidth;
        GameObject track = UiFactory.Create("Track", _bar.transform);
        Fill((RectTransform)track.transform, BarRimWidth);
        UiFactory.AddPill(track, style, HudStyle.BarTrack, trackHeight);
        GameObject fill = UiFactory.Create("Fill", track.transform);
        _barFill = (RectTransform)fill.transform;
        _barFill.anchorMin = Vector2.zero;
        _barFill.offsetMin = Vector2.zero;
        _barFill.offsetMax = Vector2.zero;
        _barFillImage = UiFactory.AddPill(fill, style, HudStyle.ProgressFill, trackHeight);

        _afflictionBar = new AfflictionBar(_statusRow.transform);
        _count = UiFactory.AddText(_statusRow.transform, "Count", style.StrongFont, StatusFontSize, HudStyle.Ink);
        // Figures stay on one line; the bar beside them gives way instead.
        _count.textWrappingMode = TextWrappingModes.NoWrap;
        _status = UiFactory.AddText(_statusRow.transform, "Label", style.StrongFont, StatusFontSize, HudStyle.ProgressFill);
        _detail = UiFactory.AddText(column.transform, "Detail", style.BodyFont, StatusFontSize, HudStyle.InkSoft);

        _onMap = new ChecklistGrid(column.transform, "Checklist", style);
        _notOnMapLabel = UiFactory.AddText(column.transform, "NotOnMapLabel", style.BodyFont, StatusFontSize, HudStyle.InkMuted);
        _notOnMapLabel.text = ModText.Get(ModTextKey.ChecklistNotOnMap);
        _notOnMap = new ChecklistGrid(column.transform, "NotOnMap", style);

        GameObject wide = UiFactory.Create("Wide", Root.transform);
        VerticalLayoutGroup wideStack = wide.AddComponent<VerticalLayoutGroup>();
        wideStack.spacing = SectionGap;
        wideStack.childControlWidth = true;
        wideStack.childControlHeight = true;
        wideStack.childForceExpandWidth = true;
        wideStack.childForceExpandHeight = false;
        Wide = wide.transform;
        wide.SetActive(false);
    }

    public GameObject Root { get; }

    /// <summary>The column under the icon, for what points at the badge's target.</summary>
    public Transform IconColumn { get; }

    /// <summary>The section across the whole card, under the icon and the text.</summary>
    public Transform Wide { get; }

    /// <param name="compact">One line: the icon, the name and the status word or reason only.</param>
    public void Show(BadgePresentation badge, TrackedBadge tracked, bool compact)
    {
        TrackedStatus status = tracked.Status;
        bool unattainable = status is TrackedStatus.Unattainable;
        bool forAlly = tracked.ForAlly && status is TrackedStatus.Achieved;
        _icon.texture = badge.Icon;
        _icon.color = badge.IsHidden || unattainable ? HudStyle.LockedIconTint : Color.white;
        _name.text = unattainable ? $"<s>{badge.Name}</s>" : badge.Name;
        _name.color = unattainable ? HudStyle.InkMuted : HudStyle.Ink;
        _description.text = badge.Description;
        _description.color = unattainable ? HudStyle.InkMuted : HudStyle.InkSoft;
        SetCompact(compact);

        Progress? progress = status switch
        {
            TrackedStatus.Attainable attainable => attainable.Progress,
            TrackedStatus.Holding holding => holding.Progress,
            TrackedStatus.Achieved achieved when !forAlly => achieved.Progress,
            _ => null,
        };
        _scopeChip.Root.SetActive(!compact && progress is { Scope: ProgressScope.Lifetime });
        _allyChip.Root.SetActive(!compact && forAlly);
        _chips.SetActive(_scopeChip.Root.activeSelf || _allyChip.Root.activeSelf);

        switch (status)
        {
            case TrackedStatus.Attainable:
                ShowMark(null, default);
                ShowProgress(progress, HudStyle.ProgressFill, warn: false, compact);
                if (progress is null)
                    ShowWhere(tracked.Detail);
                else
                    ShowLabel("", default);
                break;
            case TrackedStatus.Holding:
                ShowMark(null, default);
                // Close to its limit, an affliction bar's outline blinks and the margin left shows.
                bool near = progress is { NearLimit: true };
                ShowProgress(progress, HudStyle.ProgressFill, warn: near, compact);
                if (progress is { Unit: ProgressUnit.Percent } rate)
                    ShowLabel(near ? ModText.Format(ModTextKey.LimitLeft, rate.Target - rate.Current) : "", HudStyle.Caution);
                else if (tracked.Detail is BadgeDetail.RunClock clock)
                    ShowLabel(StatusText.Eta(clock, ClockColors), HudStyle.Ink);
                else
                    ShowLabel(ModText.Get(ModTextKey.StatusHolding), HudStyle.ProgressFill);
                break;
            case TrackedStatus.Achieved when forAlly:
                // Earned before: the run still matters to the ally, so no check mark.
                ShowMark(null, default);
                ShowProgress(null, default, warn: false, compact);
                ShowLabel(ModText.Get(ModTextKey.StatusYouHaveIt), HudStyle.InkSoft);
                break;
            case TrackedStatus.Achieved:
                ShowMark(_style.Check, HudStyle.Achieved);
                ShowProgress(progress, HudStyle.Achieved, warn: false, compact);
                ShowLabel(ModText.Get(ModTextKey.StatusAchieved), HudStyle.Achieved);
                break;
            case TrackedStatus.Unattainable blocked:
                ShowMark(_style.Cross, HudStyle.Unattainable);
                ShowProgress(null, default, warn: false, compact);
                ShowLabel(StatusText.Describe(blocked.Reason), HudStyle.Unattainable);
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(tracked), status, "Unhandled tracked status.");
        }
        ShowDetail(compact ? null : tracked.Detail, unattainable);
    }

    // Folded: a smaller icon and mark, a thinner card, the name and the status word only.
    private void SetCompact(bool compact)
    {
        float icon = compact ? CompactIconSize : IconSize;
        _iconSlot.preferredWidth = _iconSlot.minWidth = icon;
        _iconSlot.preferredHeight = _iconSlot.minHeight = icon;
        float mark = compact ? CompactMarkSize : MarkSize;
        _mark.sizeDelta = new Vector2(mark, mark);
        int paddingY = compact ? CompactPaddingY : Padding;
        _layout.padding = new RectOffset(Padding, Padding, paddingY, paddingY);
        _description.gameObject.SetActive(!compact);
        Wide.gameObject.SetActive(!compact && Wide.Cast<Transform>().Any(child => child.gameObject.activeSelf));
    }

    /// <summary>
    /// For a badge with no counter: where its biome is when it has one ("Mesa, next biome", in the
    /// progress colour once the team is in it), else "Doable".
    /// </summary>
    private void ShowWhere(BadgeDetail? detail)
    {
        if (detail is not BadgeDetail.BiomeAhead ahead)
        {
            ShowLabel(ModText.Get(ModTextKey.StatusAttainable), HudStyle.ProgressFill);
            return;
        }
        string biome = StatusText.BiomeName(ahead.Biome);
        switch (ahead.StretchesAhead)
        {
            case 0:
                ShowLabel(ModText.Format(ModTextKey.BiomeNow, biome), HudStyle.ProgressFill);
                break;
            case 1:
                ShowLabel(ModText.Format(ModTextKey.BiomeNext, biome), HudStyle.InkSoft);
                break;
            default:
                ShowLabel(ModText.Format(ModTextKey.BiomeLater, biome, ahead.StretchesAhead), HudStyle.InkSoft);
                break;
        }
    }

    private void ShowMark(Sprite? glyph, Color color)
    {
        _markRing.gameObject.SetActive(glyph != null);
        if (glyph == null)
            return;
        _markGlyph.sprite = glyph;
        _markGlyph.color = color;
        _markRing.color = color;
    }

    /// <summary>
    /// The bar and figures of a progress: drawn as its affliction's segment when it has one, else as the
    /// card's yellow bar in <paramref name="fillColor"/>. None on a folded card.
    /// </summary>
    private void ShowProgress(Progress? progress, Color fillColor, bool warn, bool compact)
    {
        _count.gameObject.SetActive(progress != null && !compact);
        if (progress is not { } shown || compact)
        {
            _bar.SetActive(false);
            _afflictionBar.Hide();
            return;
        }
        _count.text = StatusText.Count(shown);
        bool asAffliction = AfflictionBar.LookOf(shown) is { } look && _afflictionBar.Show(look, shown.Fraction, warn);
        _bar.SetActive(!asAffliction);
        if (asAffliction)
            return;
        _afflictionBar.Hide();
        _barFill.anchorMax = new Vector2(shown.Fraction, 1f);
        _barFillImage.color = fillColor;
    }

    private void ShowDetail(BadgeDetail? detail, bool unattainable)
    {
        string text = "";
        IReadOnlyList<ChecklistItem> items = [];
        switch (detail)
        {
            // Shown by the status label instead.
            case null:
            case BadgeDetail.BiomeAhead:
                break;
            // Once broken the bar is gone, so the elapsed time moves down here.
            case BadgeDetail.RunClock clock:
                text = StatusText.RunClock(clock, withElapsed: unattainable, ClockColors);
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

    /// <summary>The items this map yields first, then apart, under their label, those it does not.</summary>
    private void ShowChecklist(IReadOnlyList<ChecklistItem> items)
    {
        ChecklistItem[] notOnMap = items.Where(item => !item.OnMap).ToArray();
        _onMap.Show(items.Where(item => item.OnMap).ToArray());
        _notOnMapLabel.gameObject.SetActive(notOnMap.Length > 0);
        _notOnMap.Show(notOnMap);
    }

    private void ShowLabel(string text, Color color)
    {
        // Beside a bar the label is a word ("Earned"); alone it can be a whole reason, which wraps.
        _status.textWrappingMode = _count.gameObject.activeSelf ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
        _status.gameObject.SetActive(text.Length > 0);
        _status.text = text;
        _status.color = color;
        _statusRow.SetActive(_status.gameObject.activeSelf || _count.gameObject.activeSelf || _bar.activeSelf || _afflictionBar.Root.activeSelf);
    }

    // The stitched seam inside the card's edge, out of its layout; its edges tile, its corners stay whole.
    private static void AddSeam(Transform card, HudStyle style)
    {
        GameObject seam = UiFactory.Create("Seam", card);
        seam.AddComponent<LayoutElement>().ignoreLayout = true;
        Fill((RectTransform)seam.transform, SeamInset);
        UiFactory.AddImage(seam, style.SeamOutline, HudStyle.Seam).type = Image.Type.Tiled;
    }

    private static void Fill(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    /// <summary>A small outlined tag beside the name: "All runs", "For an ally".</summary>
    private sealed class Chip
    {
        public Chip(Transform parent, HudStyle style, string label, Color ink)
        {
            Root = UiFactory.Create("Chip", parent);
            UiFactory.AddPill(Root, style, ink, ChipHeight);
            HorizontalLayoutGroup layout = Root.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(ChipPaddingX, ChipPaddingX, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            Root.AddComponent<LayoutElement>().preferredHeight = ChipHeight;
            // The fill inside the rim, so only a thin outline of the ink shows.
            GameObject inside = UiFactory.Create("Inside", Root.transform);
            inside.AddComponent<LayoutElement>().ignoreLayout = true;
            Fill((RectTransform)inside.transform, ChipRimWidth);
            UiFactory.AddPill(inside, style, HudStyle.ChipFill, ChipHeight - 2 * ChipRimWidth);
            TextMeshProUGUI text = UiFactory.AddText(Root.transform, "Label", style.StrongFont, ChipFontSize, ink);
            text.text = label.ToUpperInvariant();
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = TextAlignmentOptions.Center;
        }

        public GameObject Root { get; }
    }

    /// <summary>A grid of checklist items, growing its cells as needed.</summary>
    private sealed class ChecklistGrid
    {
        private readonly GameObject _root;
        private readonly HudStyle _style;
        private readonly List<ChecklistCell> _cells = [];

        public ChecklistGrid(Transform parent, string name, HudStyle style)
        {
            _style = style;
            _root = UiFactory.Create(name, parent);
            GridLayoutGroup grid = _root.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(ChecklistIconSize, ChecklistIconSize);
            grid.spacing = new Vector2(ChecklistGap, ChecklistGap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = ChecklistColumns;
        }

        public void Show(IReadOnlyList<ChecklistItem> items)
        {
            _root.SetActive(items.Count > 0);
            while (_cells.Count < items.Count)
                _cells.Add(new ChecklistCell(_root.transform, _style));
            for (int i = 0; i < _cells.Count; i++)
            {
                bool used = i < items.Count;
                _cells[i].Root.SetActive(used);
                if (used)
                    _cells[i].Show(items[i]);
            }
        }
    }

    /// <summary>
    /// One item of a checklist: its icon, dimmed until eaten, then full colour with a green tick;
    /// faded further when this map does not yield it.
    /// </summary>
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
            _icon.color = item.Eaten ? Color.white : item.OnMap ? HudStyle.LockedIconTint : NotOnMapTint;
            _tick.SetActive(item.Eaten);
        }
    }
}

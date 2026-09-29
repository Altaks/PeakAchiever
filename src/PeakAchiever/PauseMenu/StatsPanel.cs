using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// The biome times of past runs, per ascent, opened from a button shown while the pause menu's badges
/// page is open: times, median and best of each biome, the whole climb of each map layout, and a
/// two-click erase of the ascent shown.
/// </summary>
internal sealed class StatsPanel
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float PanelWidth = 560f;
    private const int PanelPadding = 18;
    private const float RowGap = 4f;
    private const float TitleFontSize = 20f;
    private const float CellFontSize = 15f;
    private const float BiomeColumnWidth = 230f;
    private const float NumberColumnWidth = 96f;
    private const float TimesColumnWidth = 70f;
    private static readonly Vector2 ButtonCorner = new(1f, 0f);
    private static readonly Vector2 ButtonOffset = new(-40f, 40f);
    private const float EraseConfirmSeconds = 3f;
    // AscentUI shows ascent n from AscentData.ascents[n + 2] (v2.4.c).
    private const int AscentTitleOffset = 2;

    private readonly HudStyle _style;
    private readonly SplitHistoryStore _store;
    private readonly GameObject _openButton;
    private readonly GameObject _panel;
    private readonly TextMeshProUGUI _ascentName;
    private readonly Button _previous;
    private readonly Button _next;
    private readonly Transform _rows;
    private readonly Button _erase;
    private readonly TextMeshProUGUI _eraseLabel;
    private PauseMenuAccoladesPage? _page;
    private int _shownAscent;
    private float _eraseArmedUntil = float.NegativeInfinity;

    public StatsPanel(Transform canvas, HudStyle style, SplitHistoryStore store)
    {
        _style = style;
        _store = store;

        _openButton = UiFactory.AddButton(canvas, "StatsButton", style, ModText.Get(ModTextKey.StatsButton), Open).gameObject;
        var buttonRect = (RectTransform)_openButton.transform;
        buttonRect.anchorMin = ButtonCorner;
        buttonRect.anchorMax = ButtonCorner;
        buttonRect.pivot = ButtonCorner;
        buttonRect.anchoredPosition = ButtonOffset;
        _openButton.SetActive(false);

        _panel = UiFactory.Create("StatsPanel", canvas);
        var panelRect = (RectTransform)_panel.transform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        Image background = UiFactory.AddImage(_panel, style.RoundedRect, HudStyle.PanelBackground);
        background.type = Image.Type.Sliced;
        // Catches the clicks meant for the panel, so they do not reach the badges behind it.
        background.raycastTarget = true;
        VerticalLayoutGroup stack = _panel.AddComponent<VerticalLayoutGroup>();
        stack.padding = new RectOffset(PanelPadding, PanelPadding, PanelPadding, PanelPadding);
        stack.spacing = RowGap * 2;
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;
        ContentSizeFitter fitter = _panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        UiFactory.SetPreferredSize(_panel, PanelWidth, -1f);

        Transform header = Row(_panel.transform, "Header");
        TextMeshProUGUI title = UiFactory.AddText(header, "Title", style.DisplayFont, TitleFontSize, HudStyle.Ink);
        title.text = ModText.Get(ModTextKey.StatsTitle);
        title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        _previous = UiFactory.AddButton(header, "Previous", style, "<", () => ShowAscentAt(-1));
        _ascentName = UiFactory.AddText(header, "Ascent", style.StrongFont, CellFontSize, HudStyle.ProgressFill);
        _ascentName.alignment = TextAlignmentOptions.Center;
        _next = UiFactory.AddButton(header, "Next", style, ">", () => ShowAscentAt(1));

        _rows = UiFactory.Create("Rows", _panel.transform).transform;
        VerticalLayoutGroup rowStack = _rows.gameObject.AddComponent<VerticalLayoutGroup>();
        rowStack.spacing = RowGap;
        rowStack.childControlWidth = true;
        rowStack.childControlHeight = true;
        rowStack.childForceExpandWidth = true;
        rowStack.childForceExpandHeight = false;

        Transform footer = Row(_panel.transform, "Footer");
        _erase = UiFactory.AddButton(footer, "Erase", style, ModText.Get(ModTextKey.StatsErase), Erase);
        _eraseLabel = _erase.GetComponentInChildren<TextMeshProUGUI>();
        UiFactory.Create("Spacer", footer).AddComponent<LayoutElement>().flexibleWidth = 1f;
        UiFactory.AddButton(footer, "Close", style, ModText.Get(ModTextKey.StatsClose), Close);
        _panel.SetActive(false);
    }

    /// <summary>The badges page to follow: the button shows while it is open.</summary>
    public void Watch(PauseMenuAccoladesPage page) => _page = page;

    /// <summary>Called every frame by the overlay.</summary>
    public void Tick()
    {
        bool pageOpen = _page != null && _page.gameObject.activeInHierarchy && GUIManager.InPauseMenu;
        if (!pageOpen && _panel.activeSelf)
            Close();
        _openButton.SetActive(pageOpen && !_panel.activeSelf);
        if (!float.IsNegativeInfinity(_eraseArmedUntil) && Time.unscaledTime >= _eraseArmedUntil)
            Disarm();
    }

    private void Disarm()
    {
        _eraseArmedUntil = float.NegativeInfinity;
        _eraseLabel.text = ModText.Get(ModTextKey.StatsErase);
    }

    private void Open()
    {
        IReadOnlyList<int> ascents = _store.History.Ascents;
        _shownAscent = ascents.Contains(Ascents.currentAscent) || ascents.Count == 0 ? Ascents.currentAscent : ascents[0];
        _panel.SetActive(true);
        Fill();
    }

    private void Close()
    {
        _panel.SetActive(false);
        Disarm();
    }

    private void ShowAscentAt(int step)
    {
        IReadOnlyList<int> ascents = _store.History.Ascents;
        int index = ascents.ToList().IndexOf(_shownAscent) + step;
        if (index < 0 || index >= ascents.Count)
            return;
        _shownAscent = ascents[index];
        Disarm();
        Fill();
    }

    /// <summary>The first click arms the erase for a few seconds; the second, within them, erases.</summary>
    private void Erase()
    {
        string ascent = AscentName(_shownAscent);
        if (Time.unscaledTime >= _eraseArmedUntil)
        {
            _eraseArmedUntil = Time.unscaledTime + EraseConfirmSeconds;
            _eraseLabel.text = ModText.Format(ModTextKey.StatsEraseConfirm, ascent);
            return;
        }
        Disarm();
        bool saved = _store.EraseAscent(_shownAscent);
        Plugin.Hud.ShowToast(saved ? ModText.Format(ModTextKey.StatsErased, ascent) : ModText.Get(ModTextKey.StatsEraseFailed));
        IReadOnlyList<int> left = _store.History.Ascents;
        if (left.Count > 0)
            _shownAscent = left[0];
        Fill();
    }

    private void Fill()
    {
        foreach (Transform row in _rows)
            Object.Destroy(row.gameObject);
        IReadOnlyList<int> ascents = _store.History.Ascents;
        int index = ascents.ToList().IndexOf(_shownAscent);
        _ascentName.text = AscentName(_shownAscent);
        _previous.interactable = index > 0;
        _next.interactable = index >= 0 && index < ascents.Count - 1;

        IReadOnlyList<BiomeStats> stats = _store.History.StatsAt(_shownAscent);
        _erase.gameObject.SetActive(stats.Count > 0);
        if (stats.Count == 0)
        {
            AddLine(ModText.Get(ModTextKey.StatsEmpty), HudStyle.InkMuted);
            return;
        }
        AddCells(
            HudStyle.InkMuted,
            ModText.Get(ModTextKey.StatsColumnBiome),
            ModText.Get(ModTextKey.StatsColumnTimes),
            ModText.Get(ModTextKey.StatsColumnMedian),
            ModText.Get(ModTextKey.StatsColumnBest)
        );
        foreach (BiomeStats row in stats)
            AddCells(HudStyle.Ink, StatusText.BiomeName(row.Biome), row.Times.ToString(), StatusText.Clock(row.Median), StatusText.Clock(row.Best));

        IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> layouts = MapCatalog.Layouts;
        foreach (IReadOnlyCollection<Biome.BiomeType> layout in layouts)
        {
            if (SplitStats.TotalFor(layout, layouts, stats) is not { } total)
                continue;
            // Named by the biomes that set it apart; Shore is on every layout.
            Biome.BiomeType[] own = layout.Where(biome => !layouts.All(other => other.Contains(biome))).ToArray();
            string label = ModText.Format(ModTextKey.StatsLayoutTotal, StatusText.BiomeNames(own));
            AddCells(HudStyle.ProgressFill, label, "", StatusText.Clock(total.Median), StatusText.Clock(total.Best));
        }
    }

    private void AddCells(Color color, string biome, string times, string median, string best)
    {
        Transform row = Row(_rows, "Row");
        AddCell(row, biome, BiomeColumnWidth, TextAlignmentOptions.Left, color);
        AddCell(row, times, TimesColumnWidth, TextAlignmentOptions.Right, color);
        AddCell(row, median, NumberColumnWidth, TextAlignmentOptions.Right, color);
        AddCell(row, best, NumberColumnWidth, TextAlignmentOptions.Right, color);
    }

    private void AddCell(Transform row, string text, float width, TextAlignmentOptions alignment, Color color)
    {
        TextMeshProUGUI cell = UiFactory.AddText(row, "Cell", _style.BodyFont, CellFontSize, color);
        cell.text = text;
        cell.alignment = alignment;
        UiFactory.SetPreferredSize(cell.gameObject, width, -1f);
    }

    private void AddLine(string text, Color color) =>
        UiFactory.AddText(_rows, "Line", _style.BodyFont, CellFontSize, color).text = text;

    private static Transform Row(Transform parent, string name)
    {
        GameObject row = UiFactory.Create(name, parent);
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = RowGap * 2;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row.transform;
    }

    private static string AscentName(int ascent)
    {
        // == null, not ?.: Unity objects override the null check.
        AscentData data = SingletonAsset<AscentData>.Instance;
        List<AscentData.AscentInstanceData>? titles = data == null ? null : data.ascents;
        int index = ascent + AscentTitleOffset;
        return titles != null && index >= 0 && index < titles.Count
            ? titles[index].localizedTitle
            : ModText.Format(ModTextKey.StatsAscentFallback, ascent);
    }
}

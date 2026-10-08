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
using Object = UnityEngine.Object;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// The biome times of past runs, per ascent, on the badges page, made of the <see cref="PaperKit"/>
/// parts. Times, median and best of each biome, the whole climb of each map layout, and a two-click
/// erase of the ascent shown. Opened from a copy of the page's Back button placed under it.
/// </summary>
internal sealed class StatsPanel
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float SheetWidth = 820f;
    private const float RowFontSize = 22f;
    private const float HeadFontSize = 17f;
    private const float AscentFontSize = 24f;
    private const float BiomeColumnWidth = 330f;
    private const float TimesColumnWidth = 70f;
    private const float NumberColumnWidth = 130f;
    private const float EraseConfirmSeconds = 3f;
    // AscentUI shows ascent n from AscentData.ascents[n + 2] (v2.4.c).
    private const int AscentTitleOffset = 2;
    public const string OpenButtonName = "PeakAchiever.StatsButton";
    private const string OpenButtonLabelKey = "PEAKACHIEVER_STATISTICS";
    private const string RootName = "PeakAchiever.Statistics";
    private const float HeadAlpha = 0.7f;

    // The ribbon orange darkened to read on the paper at 5.5:1 (#A8470F there is 4.3:1).
    private static readonly Color TotalInk = new Color32(0x8C, 0x3B, 0x0D, 0xFF);

    private readonly SplitHistoryStore _store;
    private PauseMenuAccoladesPage? _page;
    private GameObject? _root;
    private TMP_Text _ascentName = null!;
    private RibbonHint _previous = null!;
    private RibbonHint _next = null!;
    private Button _erase = null!;
    private Transform _rows = null!;
    private PaperKit _kit = null!;
    private int _shownAscent;
    private float _eraseArmedUntil = float.NegativeInfinity;

    public StatsPanel(SplitHistoryStore store) => _store = store;

    /// <summary>
    /// Follows the badges page. The first time, builds the panel inside it and puts a copy of its Back
    /// button under that button, which opens the panel.
    /// </summary>
    public void Watch(PauseMenuAccoladesPage page)
    {
        _page = page;
        GameTextTable.Register(OpenButtonLabelKey, ModTextKey.StatsButton);
        if (_root != null)
            return;
        if (PaperKit.Read(page, "statistics") is not { } kit)
            return;
        _kit = kit;
        Build(page);
        PaperKit.OpenButton(page, page.backButton.transform, OpenButtonName, OpenButtonLabelKey, Open);
        PaperKit.ArrangeButtons(page);
    }

    /// <summary>Called every frame by the overlay.</summary>
    public void Tick()
    {
        if (_root == null || !_root.activeSelf)
            return;
        if (_page == null || !_page.gameObject.activeInHierarchy || !GUIManager.InPauseMenu)
            Close();
        else if (!float.IsNegativeInfinity(_eraseArmedUntil) && Time.unscaledTime >= _eraseArmedUntil)
            Disarm();
    }

    private void Build(PauseMenuAccoladesPage page)
    {
        _root = PaperKit.Veil(page.transform, RootName);
        Transform column = PaperKit.Column(_root.transform);
        Transform sheet = _kit.Sheet(column, SheetWidth);

        Transform selector = PaperKit.Row(sheet, "Ascent", TextAnchor.MiddleCenter);
        Button previous = PaperKit.Ribbon(_kit.PurpleRibbon, selector, "Previous", "<", () => ShowAscentAt(-1));
        _ascentName = _kit.Text(selector, "Name", AscentFontSize, PaperKit.PaperInk, TextAlignmentOptions.Center);
        UiFactory.SetPreferredSize(_ascentName.gameObject, BiomeColumnWidth, -1f);
        Button next = PaperKit.Ribbon(_kit.PurpleRibbon, selector, "Next", ">", () => ShowAscentAt(1));
        // The hints hang over everything else in the panel.
        _previous = RibbonHint.AttachTo(previous, _root.transform, _kit.Paper, _kit.Font, _kit.FontMaterial, PaperKit.PaperInk);
        _next = RibbonHint.AttachTo(next, _root.transform, _kit.Paper, _kit.Font, _kit.FontMaterial, PaperKit.PaperInk);

        // Eight map layouts in 2.6.b give as many total rows: past the cap, the rows scroll.
        _rows = PaperKit.ScrollList(sheet, "Rows", PaperKit.MaxListHeight);

        _kit.Title(sheet, ModText.Get(ModTextKey.StatsTitle));

        Transform actions = PaperKit.Row(column, "Actions", TextAnchor.MiddleCenter);
        _erase = PaperKit.Ribbon(_kit.PurpleRibbon, actions, "Erase", ModText.Get(ModTextKey.StatsErase), Erase);
        PaperKit.Ribbon(_kit.RedRibbon, actions, "Close", ModText.Get(ModTextKey.StatsClose), Close);
        _root.SetActive(false);
    }

    private void Open()
    {
        if (_root == null)
            return;
        IReadOnlyList<int> ascents = _store.History.Ascents;
        _shownAscent = ascents.Contains(Ascents.currentAscent) || ascents.Count == 0 ? Ascents.currentAscent : ascents[0];
        _root.transform.SetAsLastSibling();
        _root.SetActive(true);
        Fill();
    }

    private void Close()
    {
        if (_root == null)
            return;
        _root.SetActive(false);
        Disarm();
    }

    private void Disarm()
    {
        _eraseArmedUntil = float.NegativeInfinity;
        PaperKit.SetRibbonLabel(_erase.gameObject, ModText.Get(ModTextKey.StatsErase), resizeInLayout: true);
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
            PaperKit.SetRibbonLabel(_erase.gameObject, ModText.Format(ModTextKey.StatsEraseConfirm, ascent), resizeInLayout: true);
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
        FillRows();
        // The rows were just rebuilt: size the sheet to them now, not a frame later. Widths first, since
        // the rows' heights depend on them, then the list's height, then the sheet around it.
        var column = (RectTransform)_root!.transform.GetChild(0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(column);
        PaperKit.FitScrollList(_rows, PaperKit.MaxListHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(column);
    }

    private void FillRows()
    {
        // Removed at once, so the layout rebuilt right after does not count them.
        for (int i = _rows.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(_rows.GetChild(i).gameObject);
        IReadOnlyList<int> ascents = _store.History.Ascents;
        int index = ascents.ToList().IndexOf(_shownAscent);
        _ascentName.text = AscentName(_shownAscent);
        _previous.Set(index > 0, ModText.Get(ModTextKey.StatsNoLowerAscent));
        _next.Set(index >= 0 && index < ascents.Count - 1, ModText.Get(ModTextKey.StatsNoHigherAscent));

        IReadOnlyList<BiomeStats> stats = _store.History.StatsAt(_shownAscent);
        _erase.gameObject.SetActive(stats.Count > 0);
        _kit.Dots(_rows);
        if (stats.Count == 0)
        {
            _kit.Text(_rows, "Empty", RowFontSize, PaperKit.PaperInk, TextAlignmentOptions.Center, ModText.Get(ModTextKey.StatsEmpty));
            return;
        }
        var head = new Color(PaperKit.PaperInk.r, PaperKit.PaperInk.g, PaperKit.PaperInk.b, HeadAlpha);
        AddCells(
            HeadFontSize,
            head,
            ModText.Get(ModTextKey.StatsColumnBiome),
            ModText.Get(ModTextKey.StatsColumnTimes),
            ModText.Get(ModTextKey.StatsColumnMedian),
            ModText.Get(ModTextKey.StatsColumnBest)
        );
        foreach (BiomeStats row in stats)
        {
            _kit.Dots(_rows);
            AddCells(RowFontSize, PaperKit.PaperInk, StatusText.BiomeName(row.Biome), row.Times.ToString(), StatusText.Clock(row.Median), StatusText.Clock(row.Best));
        }
        IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> layouts = MapCatalog.Layouts;
        foreach (IReadOnlyCollection<Biome.BiomeType> layout in layouts)
        {
            if (SplitStats.TotalFor(layout, layouts, stats) is not { } total)
                continue;
            // Named by the biomes that set it apart; Shore is on every layout.
            Biome.BiomeType[] own = layout.Where(biome => !layouts.All(other => other.Contains(biome))).ToArray();
            _kit.Dots(_rows);
            string label = ModText.Format(ModTextKey.StatsLayoutTotal, StatusText.BiomeNames(own));
            AddCells(RowFontSize, TotalInk, label, "", StatusText.Clock(total.Median), StatusText.Clock(total.Best));
        }
    }

    private void AddCells(float size, Color color, string biome, string times, string median, string best)
    {
        Transform row = PaperKit.Row(_rows, "Row", TextAnchor.MiddleLeft);
        UiFactory.SetPreferredSize(_kit.Text(row, "Biome", size, color, TextAlignmentOptions.Left, biome).gameObject, BiomeColumnWidth, -1f);
        UiFactory.SetPreferredSize(_kit.Text(row, "Times", size, color, TextAlignmentOptions.Right, times).gameObject, TimesColumnWidth, -1f);
        UiFactory.SetPreferredSize(_kit.Text(row, "Median", size, color, TextAlignmentOptions.Right, median).gameObject, NumberColumnWidth, -1f);
        UiFactory.SetPreferredSize(_kit.Text(row, "Best", size, color, TextAlignmentOptions.Right, best).gameObject, NumberColumnWidth, -1f);
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

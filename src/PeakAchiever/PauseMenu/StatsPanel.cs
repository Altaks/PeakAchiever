using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zorro.Core;
using Object = UnityEngine.Object;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// The biome times of past runs, per ascent, on the badges page: a sheet of the badge popup's paper
/// under a copy of the page's orange title ribbon, over a dark veil, with the game's own ribbons as
/// buttons. Times, median and best of each biome, the whole climb of each map layout, and a two-click
/// erase of the ascent shown. Opened from a copy of the page's Back button placed under it.
/// </summary>
internal sealed class StatsPanel
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float SheetWidth = 820f;
    // The paper sprite has a soft torn edge and a speech-bubble tail at the bottom: keep clear of both.
    private const int SheetSidePadding = 56;
    private const int SheetBottomPadding = 64;
    // Room under the title ribbon, which straddles the sheet's top edge.
    private const int SheetTopPadding = 60;
    private const float SheetGap = 6f;
    private const float ColumnGap = 18f;
    private const float RowFontSize = 22f;
    private const float HeadFontSize = 17f;
    private const float AscentFontSize = 24f;
    private const float BiomeColumnWidth = 330f;
    private const float TimesColumnWidth = 70f;
    private const float NumberColumnWidth = 130f;
    private const float DotsHeight = 7f;
    // Space left on each side of a ribbon's label, inside the ribbon's notched ends.
    private const float RibbonSideMargin = 30f;
    private const float MinRibbonWidth = 90f;
    // The ribbon's visible banner, the child the label is centred on (the button's own rect is
    // shorter and offset, v2.4.c).
    private const string RibbonBannerName = "Panel";
    private const float EraseConfirmSeconds = 3f;
    // AscentUI shows ascent n from AscentData.ascents[n + 2] (v2.4.c).
    private const int AscentTitleOffset = 2;
    private const string OpenButtonName = "PeakAchiever.StatsButton";
    private const string OpenButtonLabelKey = "PEAKACHIEVER_STATISTICS";
    private const string RootName = "PeakAchiever.Statistics";
    private const float HeadAlpha = 0.7f;

    // Colours of the game's own UI (pause menu, v2.4.c): the blocklist popup's veil, the badge popup's ink.
    private static readonly Color Veil = new(0f, 0f, 0f, 225f / 255f);
    private static readonly Color PaperInk = new Color32(0x40, 0x35, 0x4B, 0xFF);
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
    private NativeLook _look;
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
        if (NativeLook.Read(page) is not { } look)
            return;
        _look = look;
        Build(page);
        AddOpenButton(page);
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

    private void AddOpenButton(PauseMenuAccoladesPage page)
    {
        Transform column = page.backButton.transform.parent;
        if (column.Find(OpenButtonName) != null)
            return;
        GameObject copy = Object.Instantiate(page.backButton.gameObject, column);
        copy.name = OpenButtonName;
        copy.transform.SetSiblingIndex(page.backButton.transform.GetSiblingIndex() + 1);
        Button button = copy.GetComponent<Button>();
        // A fresh event drops the copied listeners, the inspector's ones included.
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(Open);
        // == null, not a pattern: Unity objects override the null check.
        LocalizedText label = copy.GetComponentInChildren<LocalizedText>(includeInactive: true);
        if (label != null)
        {
            label.index = OpenButtonLabelKey;
            label.tmp.text = LocalizedText.GetText(OpenButtonLabelKey);
        }
    }

    private void Build(PauseMenuAccoladesPage page)
    {
        _root = UiFactory.Create(RootName, page.transform);
        Stretch((RectTransform)_root.transform);
        // The veil hides the badges and takes the clicks meant for them.
        Image veil = _root.AddComponent<Image>();
        veil.color = Veil;
        veil.raycastTarget = true;

        GameObject column = UiFactory.Create("Column", _root.transform);
        var columnRect = (RectTransform)column.transform;
        columnRect.anchorMin = columnRect.anchorMax = columnRect.pivot = new Vector2(0.5f, 0.5f);
        VerticalLayoutGroup columnLayout = column.AddComponent<VerticalLayoutGroup>();
        columnLayout.spacing = ColumnGap;
        columnLayout.childAlignment = TextAnchor.UpperCenter;
        columnLayout.childControlWidth = true;
        columnLayout.childControlHeight = true;
        columnLayout.childForceExpandWidth = false;
        columnLayout.childForceExpandHeight = false;
        ContentSizeFitter columnFit = column.AddComponent<ContentSizeFitter>();
        columnFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        columnFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject sheet = UiFactory.Create("Sheet", column.transform);
        Image paper = sheet.AddComponent<Image>();
        CopyLook(_look.Paper, paper);
        VerticalLayoutGroup sheetLayout = sheet.AddComponent<VerticalLayoutGroup>();
        sheetLayout.padding = new RectOffset(SheetSidePadding, SheetSidePadding, SheetTopPadding, SheetBottomPadding);
        sheetLayout.spacing = SheetGap;
        sheetLayout.childAlignment = TextAnchor.UpperCenter;
        sheetLayout.childControlWidth = true;
        sheetLayout.childControlHeight = true;
        sheetLayout.childForceExpandWidth = true;
        sheetLayout.childForceExpandHeight = false;
        UiFactory.SetPreferredSize(sheet, SheetWidth, -1f);

        Transform selector = Row(sheet.transform, "Ascent", TextAnchor.MiddleCenter);
        Button previous = Ribbon(_look.PurpleRibbon, selector, "Previous", "<", () => ShowAscentAt(-1));
        _ascentName = Text(selector, "Name", AscentFontSize, PaperInk, TextAlignmentOptions.Center);
        UiFactory.SetPreferredSize(_ascentName.gameObject, BiomeColumnWidth, -1f);
        Button next = Ribbon(_look.PurpleRibbon, selector, "Next", ">", () => ShowAscentAt(1));
        // The hints hang over everything else in the panel.
        _previous = RibbonHint.AttachTo(previous, _root.transform, _look.Paper, _look.Font, _look.FontMaterial, PaperInk);
        _next = RibbonHint.AttachTo(next, _root.transform, _look.Paper, _look.Font, _look.FontMaterial, PaperInk);

        _rows = UiFactory.Create("Rows", sheet.transform).transform;
        VerticalLayoutGroup rowStack = _rows.gameObject.AddComponent<VerticalLayoutGroup>();
        rowStack.spacing = SheetGap;
        rowStack.childControlWidth = true;
        rowStack.childControlHeight = true;
        rowStack.childForceExpandWidth = true;
        rowStack.childForceExpandHeight = false;

        // The title ribbon straddles the sheet's top edge, as the page's ribbons straddle their sash.
        // Last child of the sheet so it draws over it, and outside its layout.
        GameObject title = Object.Instantiate(_look.TitleRibbon, sheet.transform);
        title.name = "Title";
        title.AddComponent<LayoutElement>().ignoreLayout = true;
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        SetRibbonLabel(title, ModText.Get(ModTextKey.StatsTitle), resizeInLayout: false);

        Transform actions = Row(column.transform, "Actions", TextAnchor.MiddleCenter);
        _erase = Ribbon(_look.PurpleRibbon, actions, "Erase", ModText.Get(ModTextKey.StatsErase), Erase);
        Ribbon(_look.RedRibbon, actions, "Close", ModText.Get(ModTextKey.StatsClose), Close);
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
        SetRibbonLabel(_erase.gameObject, ModText.Get(ModTextKey.StatsErase), resizeInLayout: true);
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
            SetRibbonLabel(_erase.gameObject, ModText.Format(ModTextKey.StatsEraseConfirm, ascent), resizeInLayout: true);
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
        // The rows were just rebuilt: size the sheet to them now, not a frame later.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_rows.parent.parent);
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
        Dots(_rows);
        if (stats.Count == 0)
        {
            Text(_rows, "Empty", RowFontSize, PaperInk, TextAlignmentOptions.Center, ModText.Get(ModTextKey.StatsEmpty));
            return;
        }
        var head = new Color(PaperInk.r, PaperInk.g, PaperInk.b, HeadAlpha);
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
            Dots(_rows);
            AddCells(RowFontSize, PaperInk, StatusText.BiomeName(row.Biome), row.Times.ToString(), StatusText.Clock(row.Median), StatusText.Clock(row.Best));
        }
        IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> layouts = MapCatalog.Layouts;
        foreach (IReadOnlyCollection<Biome.BiomeType> layout in layouts)
        {
            if (SplitStats.TotalFor(layout, layouts, stats) is not { } total)
                continue;
            // Named by the biomes that set it apart; Shore is on every layout.
            Biome.BiomeType[] own = layout.Where(biome => !layouts.All(other => other.Contains(biome))).ToArray();
            Dots(_rows);
            string label = ModText.Format(ModTextKey.StatsLayoutTotal, StatusText.BiomeNames(own));
            AddCells(RowFontSize, TotalInk, label, "", StatusText.Clock(total.Median), StatusText.Clock(total.Best));
        }
    }

    private void AddCells(float size, Color color, string biome, string times, string median, string best)
    {
        Transform row = Row(_rows, "Row", TextAnchor.MiddleLeft);
        UiFactory.SetPreferredSize(Text(row, "Biome", size, color, TextAlignmentOptions.Left, biome).gameObject, BiomeColumnWidth, -1f);
        UiFactory.SetPreferredSize(Text(row, "Times", size, color, TextAlignmentOptions.Right, times).gameObject, TimesColumnWidth, -1f);
        UiFactory.SetPreferredSize(Text(row, "Median", size, color, TextAlignmentOptions.Right, median).gameObject, NumberColumnWidth, -1f);
        UiFactory.SetPreferredSize(Text(row, "Best", size, color, TextAlignmentOptions.Right, best).gameObject, NumberColumnWidth, -1f);
    }

    /// <summary>A dotted rule, the badge popup's own separator.</summary>
    private void Dots(Transform parent)
    {
        GameObject rule = UiFactory.Create("Dots", parent);
        Image dots = rule.AddComponent<Image>();
        CopyLook(_look.Dots, dots);
        UiFactory.SetPreferredSize(rule, -1f, DotsHeight);
    }

    private TMP_Text Text(Transform parent, string name, float size, Color color, TextAlignmentOptions alignment, string text = "")
    {
        TextMeshProUGUI label = UiFactory.AddText(parent, name, _look.Font, size, color);
        label.fontSharedMaterial = _look.FontMaterial;
        label.alignment = alignment;
        label.text = text;
        return label;
    }

    private static void CopyLook(Image source, Image target)
    {
        target.sprite = source.sprite;
        target.type = source.type;
        target.material = source.material;
        target.color = source.color;
        target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        target.raycastTarget = false;
    }

    /// <summary>A copy of one of the game's ribbon buttons with the mod's label and action.</summary>
    private static Button Ribbon(Button source, Transform parent, string name, string label, UnityAction onClick)
    {
        GameObject copy = Object.Instantiate(source.gameObject, parent);
        copy.name = name;
        Button button = copy.GetComponent<Button>();
        // A fresh event drops the copied listeners, the inspector's ones included.
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(onClick);
        // The mod sets these labels itself, some while the panel is open.
        foreach (LocalizedText localized in copy.GetComponentsInChildren<LocalizedText>(includeInactive: true))
            Object.DestroyImmediate(localized);
        SetRibbonLabel(copy, label, resizeInLayout: true);
        return button;
    }

    /// <summary>
    /// Sets a ribbon's label, laid over the ribbon's visible banner between its notched ends and centred
    /// on both axes; the ribbon is as wide as the label needs, never below a small minimum.
    /// </summary>
    private static void SetRibbonLabel(GameObject ribbon, string label, bool resizeInLayout)
    {
        TMP_Text text = ribbon.GetComponentInChildren<TMP_Text>(includeInactive: true);
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        var textRect = text.rectTransform;
        // Same anchors and edges as the banner, so the label's centre is the banner's centre.
        var banner = ribbon.transform.Find(RibbonBannerName) as RectTransform;
        textRect.anchorMin = banner != null ? banner.anchorMin : Vector2.zero;
        textRect.anchorMax = banner != null ? banner.anchorMax : Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        Vector2 bannerMin = banner != null ? banner.offsetMin : Vector2.zero;
        Vector2 bannerMax = banner != null ? banner.offsetMax : Vector2.zero;
        textRect.offsetMin = bannerMin + new Vector2(RibbonSideMargin, 0f);
        textRect.offsetMax = bannerMax - new Vector2(RibbonSideMargin, 0f);

        var rect = (RectTransform)ribbon.transform;
        float width = Mathf.Max(MinRibbonWidth, text.GetPreferredValues(label).x + 2 * RibbonSideMargin);
        if (!resizeInLayout)
        {
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
            return;
        }
        LayoutElement layout = ribbon.GetComponent<LayoutElement>();
        if (layout == null)
            layout = ribbon.AddComponent<LayoutElement>();
        // The native height, read once: later calls see the height the layout gave it.
        if (layout.preferredHeight <= 0f)
            layout.preferredHeight = rect.rect.height;
        layout.preferredWidth = width;
    }

    private static Transform Row(Transform parent, string name, TextAnchor alignment)
    {
        GameObject row = UiFactory.Create(name, parent);
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = SheetGap * 2;
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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

    /// <summary>
    /// The game's own UI parts the panel is made of, read from the pause menu (v2.4.c): the badge popup's
    /// paper, dotted rule and text face, the badges page's title ribbon and Back button, and the Controls
    /// page's Reset Defaults ribbon.
    /// </summary>
    private readonly record struct NativeLook(
        Image Paper,
        Image Dots,
        TMP_FontAsset Font,
        Material FontMaterial,
        GameObject TitleRibbon,
        Button RedRibbon,
        Button PurpleRibbon
    )
    {
        private const string PaperSprite = "BadgePopup";
        private const string DotsSprite = "DottedLine";
        private const string TitleRibbonName = "UI_ScoutTitle";

        /// <returns>Null, with a warning, when the game no longer has one of them.</returns>
        public static NativeLook? Read(PauseMenuAccoladesPage page)
        {
            // == null throughout: Unity objects override the null check.
            BadgeManager badges = page.GetComponentInChildren<BadgeManager>(includeInactive: true);
            GameObject? popup = badges == null ? null : badges.badgePopup;
            Image[] popupImages = popup == null ? [] : popup.GetComponentsInChildren<Image>(includeInactive: true);
            Image? paper = popupImages.FirstOrDefault(image => image.sprite != null && image.sprite.name == PaperSprite && image.type == Image.Type.Sliced);
            Image? dots = popupImages.FirstOrDefault(image => image.sprite != null && image.sprite.name == DotsSprite);
            TextMeshProUGUI? face = badges == null ? null : badges.badgePopupName;
            Transform title = page.transform.Find(TitleRibbonName);
            PauseMenuControlsPage controls = page.GetComponentInParent<Canvas>(includeInactive: true)
                .rootCanvas.GetComponentInChildren<PauseMenuControlsPage>(includeInactive: true);
            if (paper == null || dots == null || face == null || title == null || controls == null)
            {
                Plugin.Log.LogWarning("The pause menu is missing one of the parts the statistics panel is made of; the panel is left out.");
                return null;
            }
            return new NativeLook(paper, dots, face.font, face.fontSharedMaterial, title.gameObject, page.backButton, controls.restoreAllButton);
        }
    }
}

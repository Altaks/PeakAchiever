using System.Linq;
using PeakAchiever.Hud;
using PeakAchiever.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// The parts the mod's panels on the badges page are made of, all taken from PEAK's own pause menu
/// (v2.4.c): a dark veil, a sheet of the badge popup's paper with its dotted rules and text face, the
/// page's orange title ribbon, the red Back and purple Reset Defaults ribbons as buttons.
/// </summary>
internal sealed class PaperKit
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    // The paper sprite has a soft torn edge and a speech-bubble tail at the bottom: keep clear of both.
    public const int SheetSidePadding = 56;
    public const int SheetBottomPadding = 64;
    // Room under the title ribbon, which straddles the sheet's top edge.
    public const int SheetTopPadding = 60;
    public const float Gap = 6f;
    private const float ColumnGap = 18f;
    private const float DotsHeight = 7f;
    // Space left on each side of a ribbon's label, inside the ribbon's notched ends.
    private const float RibbonSideMargin = 30f;
    private const float MinRibbonWidth = 90f;
    // The ribbon's visible banner, the child the label is centred on (the button's own rect is
    // shorter and offset).
    private const string RibbonBannerName = "Panel";
    private const string PaperSprite = "BadgePopup";
    private const string DotsSprite = "DottedLine";
    private const string TitleRibbonName = "UI_ScoutTitle";

    // Colours of the game's own UI: the blocklist popup's veil, the badge popup's ink.
    private static readonly Color VeilColour = new(0f, 0f, 0f, 225f / 255f);
    public static readonly Color PaperInk = new Color32(0x40, 0x35, 0x4B, 0xFF);

    private PaperKit(Image paper, Image dots, TMP_FontAsset font, Material fontMaterial, GameObject titleRibbon, Button redRibbon, Button purpleRibbon)
    {
        Paper = paper;
        DotsLook = dots;
        Font = font;
        FontMaterial = fontMaterial;
        TitleRibbonSource = titleRibbon;
        RedRibbon = redRibbon;
        PurpleRibbon = purpleRibbon;
    }

    public Image Paper { get; }
    public TMP_FontAsset Font { get; }
    public Material FontMaterial { get; }
    public Button RedRibbon { get; }
    public Button PurpleRibbon { get; }
    private Image DotsLook { get; }
    private GameObject TitleRibbonSource { get; }

    /// <returns>Null, with a warning naming <paramref name="panel"/>, when the game no longer has one of the parts.</returns>
    public static PaperKit? Read(PauseMenuAccoladesPage page, string panel)
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
            Plugin.Log.LogWarning($"The pause menu is missing one of the parts the {panel} panel is made of; the panel is left out.");
            return null;
        }
        return new PaperKit(paper, dots, face.font, face.fontSharedMaterial, title.gameObject, page.backButton, controls.restoreAllButton);
    }

    /// <summary>A full-page veil that hides the badges and takes the clicks meant for them.</summary>
    public static GameObject Veil(Transform page, string name)
    {
        GameObject root = UiFactory.Create(name, page);
        Stretch((RectTransform)root.transform);
        Image veil = root.AddComponent<Image>();
        veil.color = VeilColour;
        veil.raycastTarget = true;
        return root;
    }

    /// <summary>A centred column that sizes to its content, for the sheet and the buttons under it.</summary>
    public static Transform Column(Transform veil)
    {
        GameObject column = UiFactory.Create("Column", veil);
        var rect = (RectTransform)column.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        VerticalLayoutGroup layout = column.AddComponent<VerticalLayoutGroup>();
        layout.spacing = ColumnGap;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fit = column.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return column.transform;
    }

    /// <summary>A sheet of the badge popup's paper, <paramref name="width"/> wide, stacking its children.</summary>
    public Transform Sheet(Transform column, float width)
    {
        GameObject sheet = UiFactory.Create("Sheet", column);
        CopyLook(Paper, sheet.AddComponent<Image>());
        VerticalLayoutGroup layout = sheet.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(SheetSidePadding, SheetSidePadding, SheetTopPadding, SheetBottomPadding);
        layout.spacing = Gap;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        UiFactory.SetPreferredSize(sheet, width, -1f);
        return sheet.transform;
    }

    /// <summary>
    /// A copy of the page's orange title ribbon straddling the sheet's top edge, as the page's ribbons
    /// straddle their sash. Call it last, so it draws over the sheet; it stays out of the sheet's layout.
    /// </summary>
    public void Title(Transform sheet, string label)
    {
        GameObject title = Object.Instantiate(TitleRibbonSource, sheet);
        title.name = "Title";
        title.AddComponent<LayoutElement>().ignoreLayout = true;
        var rect = (RectTransform)title.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        SetRibbonLabel(title, label, resizeInLayout: false);
    }

    /// <summary>A copy of one of the game's ribbon buttons with the mod's label and action.</summary>
    public static Button Ribbon(Button source, Transform parent, string name, string label, UnityAction onClick)
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
    public static void SetRibbonLabel(GameObject ribbon, string label, bool resizeInLayout)
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

    /// <summary>
    /// Puts a copy of the page's Back button right after <paramref name="after"/> in its column, once;
    /// its label goes through the game's text table under <paramref name="labelKey"/>.
    /// </summary>
    public static GameObject OpenButton(PauseMenuAccoladesPage page, Transform after, string name, string labelKey, UnityAction onClick)
    {
        Transform column = page.backButton.transform.parent;
        Transform existing = column.Find(name);
        if (existing != null)
            return existing.gameObject;
        GameObject copy = Object.Instantiate(page.backButton.gameObject, column);
        copy.name = name;
        copy.transform.SetSiblingIndex(after.GetSiblingIndex() + 1);
        Button button = copy.GetComponent<Button>();
        // A fresh event drops the copied listeners, the inspector's ones included.
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(onClick);
        // == null, not a pattern: Unity objects override the null check.
        LocalizedText label = copy.GetComponentInChildren<LocalizedText>(includeInactive: true);
        if (label != null)
        {
            label.index = labelKey;
            label.tmp.text = LocalizedText.GetText(labelKey);
        }
        return copy;
    }

    /// <summary>A dotted rule, the badge popup's own separator.</summary>
    public void Dots(Transform parent)
    {
        GameObject rule = UiFactory.Create("Dots", parent);
        CopyLook(DotsLook, rule.AddComponent<Image>());
        UiFactory.SetPreferredSize(rule, -1f, DotsHeight);
    }

    /// <summary>Text in the badge popup's own face.</summary>
    public TMP_Text Text(Transform parent, string name, float size, Color color, TextAlignmentOptions alignment, string text = "")
    {
        TextMeshProUGUI label = UiFactory.AddText(parent, name, Font, size, color);
        label.fontSharedMaterial = FontMaterial;
        label.alignment = alignment;
        label.text = text;
        return label;
    }

    public static void CopyLook(Image source, Image target)
    {
        target.sprite = source.sprite;
        target.type = source.type;
        target.material = source.material;
        target.color = source.color;
        target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        target.raycastTarget = false;
    }

    public static Transform Row(Transform parent, string name, TextAnchor alignment)
    {
        GameObject row = UiFactory.Create(name, parent);
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = Gap * 2;
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row.transform;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

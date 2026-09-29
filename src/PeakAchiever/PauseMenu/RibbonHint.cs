using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// Greys out one of the game's ribbon buttons while it cannot be used, and, on hover, shows why in a
/// small sheet of the badge popup's paper whose tail points at the ribbon.
/// </summary>
internal sealed class RibbonHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float HintWidth = 460f;
    // The paper's torn edge takes room inside its rect, as on the statistics sheet.
    private const int HintSidePadding = 40;
    private const int HintTopPadding = 26;
    // The paper's tail sits in its bottom border: leave it room under the text.
    private const int HintTailPadding = 40;
    private const float HintFontSize = 17f;
    private const float HintGap = 4f;

    // The ribbon's colours when it cannot be used: its banner, and its dotted lines.
    private static readonly Color GreyBanner = new Color32(0x6E, 0x6A, 0x66, 0xFF);
    private static readonly Color GreyDots = new Color32(0x9A, 0x95, 0x8F, 0xFF);
    private const string BannerName = "Panel";
    private const string DotsName = "Border";

    private readonly Dictionary<Image, Color> _nativeColours = [];
    private Button _button = null!;
    private GameObject _hint = null!;
    private TMP_Text _hintText = null!;
    private string _reason = "";

    public static RibbonHint AttachTo(Button ribbon, Transform hintLayer, Image paper, TMP_FontAsset font, Material fontMaterial, Color ink)
    {
        RibbonHint hint = ribbon.gameObject.AddComponent<RibbonHint>();
        hint._button = ribbon;
        foreach (Image image in ribbon.GetComponentsInChildren<Image>(includeInactive: true).Where(image => image.name is BannerName or DotsName))
            hint._nativeColours[image] = image.color;

        hint._hint = new GameObject("Hint", typeof(RectTransform));
        hint._hint.transform.SetParent(hintLayer, worldPositionStays: false);
        Image sheet = hint._hint.AddComponent<Image>();
        sheet.sprite = paper.sprite;
        sheet.type = paper.type;
        sheet.material = paper.material;
        sheet.color = paper.color;
        sheet.pixelsPerUnitMultiplier = paper.pixelsPerUnitMultiplier;
        sheet.raycastTarget = false;
        VerticalLayoutGroup layout = hint._hint.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(HintSidePadding, HintSidePadding, HintTopPadding, HintTailPadding);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        ContentSizeFitter fit = hint._hint.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var rect = (RectTransform)hint._hint.transform;
        rect.sizeDelta = new Vector2(HintWidth, rect.sizeDelta.y);
        // Hangs above the ribbon, its tail at the bottom pointing down at it.
        rect.pivot = new Vector2(0.5f, 0f);

        var text = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        text.transform.SetParent(hint._hint.transform, worldPositionStays: false);
        text.font = font;
        text.fontSharedMaterial = fontMaterial;
        text.fontSize = HintFontSize;
        text.color = ink;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        hint._hintText = text;
        hint._hint.SetActive(false);
        return hint;
    }

    /// <summary>Usable as usual, or greyed out with the reason shown on hover.</summary>
    public void Set(bool usable, string reason)
    {
        _button.interactable = usable;
        _reason = reason;
        foreach (KeyValuePair<Image, Color> native in _nativeColours)
            native.Key.color = usable ? native.Value : native.Key.name == DotsName ? GreyDots : GreyBanner;
        if (usable)
            _hint.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable)
            return;
        _hintText.text = _reason;
        var ribbon = (RectTransform)transform;
        var hint = (RectTransform)_hint.transform;
        // Placed over the ribbon's top edge, in the hint layer's space.
        Vector3 top = ribbon.TransformPoint(new Vector3(ribbon.rect.center.x, ribbon.rect.yMax + HintGap, 0f));
        hint.position = top;
        _hint.transform.SetAsLastSibling();
        _hint.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) => _hint.SetActive(false);

    private void OnDisable()
    {
        if (_hint != null)
            _hint.SetActive(false);
    }
}

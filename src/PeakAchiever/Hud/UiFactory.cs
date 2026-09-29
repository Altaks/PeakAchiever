using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Hud;

/// <summary>Builds the uGUI objects of the tracker in code, since the mod ships no prefab.</summary>
internal static class UiFactory
{
    private const int ButtonPaddingX = 12;
    private const int ButtonPaddingY = 6;
    private const float ButtonFontSize = 14f;

    public static GameObject Create(string name, Transform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, worldPositionStays: false);
        return gameObject;
    }

    public static Image AddImage(GameObject target, Sprite sprite, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static TextMeshProUGUI AddText(Transform parent, string name, TMP_FontAsset font, float size, Color color)
    {
        TextMeshProUGUI text = Create(name, parent).AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>Fixes a layout child's size; a negative value leaves that axis to the layout.</summary>
    public static void SetPreferredSize(GameObject target, float width, float height)
    {
        // TryGetComponent, not '??': Unity objects override null checks, which '??' bypasses.
        if (!target.TryGetComponent(out LayoutElement layout))
            layout = target.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
    }

    public static void SetFixedSize(GameObject target, float width, float height)
    {
        SetPreferredSize(target, width, height);
        LayoutElement layout = target.GetComponent<LayoutElement>();
        layout.minWidth = width;
        layout.minHeight = height;
    }

    /// <summary>Places a square child of <paramref name="size"/> on a point of its parent, outside any layout.</summary>
    public static void PinToCorner(GameObject target, Vector2 anchor, Vector2 offset, float size)
    {
        var rect = (RectTransform)target.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(size, size);
    }

    /// <summary>
    /// A clickable label on a rounded background. Its layout group reports the label's size plus padding,
    /// so the layout it sits in sizes it; a ContentSizeFitter here would fight that layout.
    /// </summary>
    public static Button AddButton(Transform parent, string name, HudStyle style, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject root = Create(name, parent);
        Image background = AddImage(root, style.RoundedRect, HudStyle.ButtonBackground);
        background.type = Image.Type.Sliced;
        background.raycastTarget = true;
        root.AddComponent<HorizontalLayoutGroup>().padding = new RectOffset(ButtonPaddingX, ButtonPaddingX, ButtonPaddingY, ButtonPaddingY);
        TextMeshProUGUI text = AddText(root.transform, "Label", style.StrongFont, ButtonFontSize, HudStyle.Ink);
        text.text = label;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);
        return button;
    }

    /// <summary>
    /// A pill of this height: half-circle ends at any width, like a CSS border-radius of 50%. The sliced
    /// borders are scaled to exactly half the height (Image.pixelsPerUnitMultiplier divides them).
    /// </summary>
    public static Image AddPill(GameObject target, HudStyle style, Color color, float height)
    {
        Image image = AddImage(target, style.Pill, color);
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = HudStyle.PillBorder / (height / 2f);
        return image;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Hud;

/// <summary>Builds the uGUI objects of the tracker in code, since the mod ships no prefab.</summary>
internal static class UiFactory
{
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
}

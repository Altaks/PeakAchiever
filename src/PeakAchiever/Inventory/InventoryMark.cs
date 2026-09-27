using PeakAchiever.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Inventory;

internal enum InventoryMarkKind
{
    /// <summary>Using this item breaks a pinned badge.</summary>
    Forbidden,

    /// <summary>The backpack holds at least one forbidden item.</summary>
    HoldsForbidden,
}

/// <summary>The small mark in the bottom-right corner of an inventory slot, as in the signed-off mockup.</summary>
internal static class InventoryMark
{
    private const string MarkName = "PeakAchiever.InventoryMark";
    private const float MarkSize = 26f;
    private const float RingWidth = 2f;
    private const float GlyphSize = 14f;
    private static readonly Vector2 CornerOffset = new(6f, -6f);

    /// <param name="kind">Null clears the slot's mark.</param>
    public static void Show(Transform slot, InventoryMarkKind? kind)
    {
        Transform? existing = slot.Find(MarkName);
        if (kind is not { } shown)
        {
            if (existing != null)
                existing.gameObject.SetActive(false);
            return;
        }
        if (Plugin.Hud.Style is not { } style)
            return;
        GameObject mark = existing != null ? existing.gameObject : Create(slot, style);
        mark.SetActive(true);
        Color color = shown == InventoryMarkKind.Forbidden ? HudStyle.Unattainable : HudStyle.ProgressFill;
        mark.GetComponent<Image>().color = color;
        Image glyph = mark.transform.Find("Glyph").GetComponent<Image>();
        glyph.sprite = shown == InventoryMarkKind.Forbidden ? style.Cross : style.Warning;
        glyph.color = color;
    }

    private static GameObject Create(Transform slot, HudStyle style)
    {
        GameObject mark = UiFactory.Create(MarkName, slot);
        UiFactory.PinToCorner(mark, new Vector2(1f, 0f), CornerOffset, MarkSize);
        UiFactory.AddImage(mark, style.Circle, Color.white);
        GameObject inside = UiFactory.Create("Inside", mark.transform);
        UiFactory.PinToCorner(inside, new Vector2(0.5f, 0.5f), Vector2.zero, MarkSize - 2 * RingWidth);
        UiFactory.AddImage(inside, style.Circle, HudStyle.MarkBackground);
        GameObject glyph = UiFactory.Create("Glyph", mark.transform);
        UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, GlyphSize);
        UiFactory.AddImage(glyph, style.Cross, Color.white).preserveAspect = true;
        // Drawn over the slot's own icon and outline.
        mark.transform.SetAsLastSibling();
        return mark;
    }
}

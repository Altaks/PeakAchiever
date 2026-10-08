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
/// What a badge needs, as dark tabs hung under the card's bottom edge and aligned right: one for the needed
/// items ("Needs" or "One of"), one for the helpful ones ("Helps"), each item as the game's inventory icon
/// with its count (signed-off sketch).
/// </summary>
internal sealed class NeedsBlock
{
    // Sizes in reference pixels of the 1920x1080 canvas, taken from the signed-off mockup.
    private const float TileSize = 26f;
    private const float TileGap = 4f;
    private const float HeadFontSize = 10f;
    private const float HeadGap = 6f;
    private const float CountFontSize = 9.5f;
    private const float TabGap = 6f;
    // The tabs sit this far in from the card's right edge, clear of its rounded corner and seam.
    private const int TabInset = 14;
    private const int TabPaddingX = 8;
    private const int TabPaddingY = 5;
    // Darker than the card, so the tabs read as their own pieces.
    private static readonly Color TabBackground = new(14f / 255, 11f / 255, 9f / 255, 0.92f);

    /// <summary>How far the tabs overlap the card's bottom edge.</summary>
    public const float Overlap = 4f;
    private const float CountPadding = 2f;
    private static readonly Color TileBackground = new(HudStyle.Ink.r, HudStyle.Ink.g, HudStyle.Ink.b, 0.1f);

    private readonly HudStyle _style;
    private ACHIEVEMENTTYPE? _shown;

    public NeedsBlock(Transform parent, HudStyle style)
    {
        _style = style;
        Root = UiFactory.Create("Needs", parent);
        HorizontalLayoutGroup tabs = Root.AddComponent<HorizontalLayoutGroup>();
        tabs.spacing = TabGap;
        tabs.padding = new RectOffset(0, TabInset, 0, 0);
        tabs.childAlignment = TextAnchor.UpperRight;
        tabs.childControlWidth = true;
        tabs.childControlHeight = true;
        tabs.childForceExpandWidth = false;
        tabs.childForceExpandHeight = false;
        Root.SetActive(false);
    }

    public GameObject Root { get; }

    /// <summary>True once the shown badge has at least one item the game knows.</summary>
    public bool HasItems => Root.transform.childCount > 0 && Root.transform.Cast<Transform>().Any(tab => tab.gameObject.activeSelf);

    /// <summary>Built once per badge: the needs never change during a run.</summary>
    public void Show(ACHIEVEMENTTYPE badge)
    {
        if (_shown == badge)
            return;
        _shown = badge;
        // DestroyImmediate, not Destroy: HasItems reads the children in this same frame.
        for (int i = Root.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(Root.transform.GetChild(i).gameObject);
        BadgeNeed? need = BadgeNeeds.For(badge);
        Root.SetActive(need != null);
        if (need == null)
            return;
        if (need.Items.Count > 0)
            Column(ModText.Get(need.Kind == NeedKind.OneOf ? ModTextKey.NeedsOneOf : ModTextKey.NeedsAll), need.Items);
        if (need.Helps.Count > 0)
            Column(ModText.Get(ModTextKey.NeedsHelps), need.Helps);
    }

    private void Column(string head, IReadOnlyList<NeededItem> items)
    {
        // Items the game no longer has are left out, and with all of them, the column.
        (Item Item, int Count)[] found = items
            .Select(needed => (Item: ItemCatalog.ByName(needed.ItemName), needed.Count))
            .Where(entry => entry.Item != null)
            .Select(entry => (entry.Item!, entry.Count))
            .ToArray();
        if (found.Length == 0)
            return;
        Plugin.Log.LogDebug($"{head} {string.Join(", ", found.Select(entry => $"{entry.Item.name} x{entry.Count}"))}");
        // The label and its icons on one line, the label centred on the icons. The longest list (Rule Zero,
        // six items) fits the card's width, so the row never needs to wrap.
        GameObject column = UiFactory.Create("Tab", Root.transform);
        UiFactory.AddImage(column, _style.RoundedRect, TabBackground).type = Image.Type.Sliced;
        HorizontalLayoutGroup row = column.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(TabPaddingX, TabPaddingX, TabPaddingY, TabPaddingY);
        row.spacing = HeadGap;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;
        TextMeshProUGUI label = UiFactory.AddText(column.transform, "Head", _style.StrongFont, HeadFontSize, HudStyle.InkSoft);
        label.text = head;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        GameObject tiles = UiFactory.Create("Tiles", column.transform);
        HorizontalLayoutGroup tileRow = tiles.AddComponent<HorizontalLayoutGroup>();
        tileRow.spacing = TileGap;
        tileRow.childAlignment = TextAnchor.MiddleLeft;
        tileRow.childControlWidth = true;
        tileRow.childControlHeight = true;
        tileRow.childForceExpandWidth = false;
        tileRow.childForceExpandHeight = false;
        foreach ((Item item, int count) in found)
            Tile(tiles.transform, item, count);
    }

    private void Tile(Transform parent, Item item, int count)
    {
        GameObject tile = UiFactory.Create("Item", parent);
        UiFactory.SetFixedSize(tile, TileSize, TileSize);
        UiFactory.AddImage(tile, _style.RoundedRect, TileBackground).type = Image.Type.Sliced;
        GameObject icon = UiFactory.Create("Icon", tile.transform);
        var iconRect = (RectTransform)icon.transform;
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        RawImage image = icon.AddComponent<RawImage>();
        image.texture = item.UIData.icon;
        image.raycastTarget = false;
        if (count <= 1)
            return;
        TextMeshProUGUI label = UiFactory.AddText(tile.transform, "Count", _style.StrongFont, CountFontSize, HudStyle.Ink);
        label.text = ModText.Format(ModTextKey.NeedsCount, count);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.alignment = TextAlignmentOptions.BottomRight;
        var countRect = label.rectTransform;
        countRect.anchorMin = Vector2.zero;
        countRect.anchorMax = Vector2.one;
        countRect.offsetMin = new Vector2(0f, -CountPadding);
        countRect.offsetMax = new Vector2(CountPadding * 2, 0f);
        label.outlineWidth = 0.2f;
        label.outlineColor = HudStyle.MarkBackground;
    }
}

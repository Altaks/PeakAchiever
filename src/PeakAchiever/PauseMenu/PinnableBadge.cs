using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Localization;
using PeakAchiever.Pinning;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// Added to each badge of the pause menu's badges page: a click pins or unpins it,
/// and a pin marker shows on the badges already pinned.
/// </summary>
internal sealed class PinnableBadge : MonoBehaviour
{
    // Mockup: a 22 px marker overlapping the badge's top-right edge by 4 px; the map cross mirrors it
    // on the bottom-right edge.
    private const float MarkerSize = 22f;
    private const float MarkerGlyphSize = 12f;
    private static readonly Vector2 MarkerOffset = new(4f, 4f);
    private static readonly Vector2 NotOnMapOffset = new(4f, -4f);
    private static readonly Vector2 SuggestedOffset = new(-4f, 4f);
    private static int _suggestionsFrame = -1;
    private static IReadOnlyList<ACHIEVEMENTTYPE> _suggestions = [];
    // Faded enough to read as unavailable next to the other locked badges, still legible.
    private const float ConflictAlpha = 0.35f;

    private BadgeUI _badge = null!;
    private GameObject? _marker;
    private GameObject? _notOnMap;
    private GameObject? _suggested;
    private CanvasGroup _fade = null!;

    /// <summary>Hooks the badge's own Button, so mouse clicks and gamepad submit both toggle the pin.</summary>
    public static void AttachTo(BadgeUI badge)
    {
        if (!badge.TryGetComponent(out PinnableBadge pinnable))
        {
            if (!badge.TryGetComponent(out Button button))
            {
                Plugin.Log.LogError($"Badge '{badge.name}' has no Button; it cannot be pinned from the pause menu.");
                return;
            }
            pinnable = badge.gameObject.AddComponent<PinnableBadge>();
            pinnable._badge = badge;
            // BadgeUI declares a CanvasGroup the game's code never drives (v2.4.c); add one when unset.
            pinnable._fade = badge.canvasGroup != null ? badge.canvasGroup : badge.gameObject.AddComponent<CanvasGroup>();
            button.onClick.AddListener(pinnable.TogglePin);
        }
        pinnable.ShowPinState();
    }

    /// <summary>The first pinned badge this one cannot share a run with, if any.</summary>
    public static ACHIEVEMENTTYPE? ConflictOf(ACHIEVEMENTTYPE badge) =>
        MapCatalog.Compatibility.FirstConflict(badge, Plugin.Pins.Board.Pins);

    /// <summary>
    /// The badges to star on the page: as many as there are free pin slots, allowed by the known map and
    /// compatible with the pins and each other (<see cref="PinSuggestions"/>). A locked secret badge is
    /// never starred, since that would tell what it is. Worked out once per frame for the whole page.
    /// </summary>
    public static IReadOnlyList<ACHIEVEMENTTYPE> Suggestions(BadgeManager manager)
    {
        if (_suggestionsFrame == Time.frameCount)
            return _suggestions;
        _suggestionsFrame = Time.frameCount;
        PinBoard board = Plugin.Pins.Board;
        _suggestions = MapCatalog.CurrentOrToday is { } map
            ? PinSuggestions.For(
                manager.badgeData.Where(data => data != null && data.IsLocked && !data.secret).Select(data => data.linkedAchievement),
                board.Pins.ToArray(),
                board.Capacity - board.Pins.Count,
                map.Biomes,
                MapCatalog.Compatibility
            )
            : [];
        return _suggestions;
    }

    /// <summary>The biomes the run's map, or today's in the airport, lacks for this badge; empty when it has them.</summary>
    public static IReadOnlyCollection<Biome.BiomeType> MissingOnKnownMap(ACHIEVEMENTTYPE badge) =>
        MapCatalog.CurrentOrToday is { } map ? MapCatalog.Compatibility.MissingOn(badge, map.Biomes) : [];

    private void TogglePin()
    {
        BadgeData? data = _badge.data;
        if (data == null)
            return;
        PinnedBadgesStore store = Plugin.Pins;
        PinToggleOutcome outcome = store.Board.Toggle(data.linkedAchievement, isEarned: !data.IsLocked, MapCatalog.Compatibility);
        switch (outcome)
        {
            case PinToggleOutcome.Pinned:
            case PinToggleOutcome.Unpinned:
                store.Save();
                Plugin.Hud.RequestRefresh();
                // A pin change can fade or restore any badge of the page.
                foreach (PinnableBadge badge in _badge.manager.GetComponentsInChildren<PinnableBadge>(includeInactive: true))
                    badge.ShowPinState();
                // Re-selecting re-runs the game's popup, so the click hint flips between pin and unpin.
                _badge.manager.selectedBadge = _badge;
                break;
            case PinToggleOutcome.RejectedBoardFull:
                Plugin.Hud.ShowToast(ModText.Format(ModTextKey.RefusalBoardFull, store.Board.Capacity));
                break;
            case PinToggleOutcome.RejectedConflict:
                ACHIEVEMENTTYPE conflict = ConflictOf(data.linkedAchievement)!.Value;
                Plugin.Hud.ShowToast(ModText.Format(ModTextKey.RefusalConflict, BadgeCatalog.Present(conflict).Name));
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled pin outcome.");
        }
    }

    /// <summary>
    /// The pin marker on a pinned badge, a red cross on one the map cannot hold, a star on a suggested
    /// one, and a fade on one that conflicts with a pin.
    /// </summary>
    private void ShowPinState()
    {
        BadgeData? data = _badge.data;
        bool pinned = data != null && Plugin.Pins.Board.IsPinned(data.linkedAchievement);
        if (pinned && _marker == null && Plugin.Hud.Style is { } style)
            _marker = CreateMarker(style, "PinMarker", Vector2.one, MarkerOffset, HudStyle.ProgressFill, style.Pin);
        if (_marker != null)
            _marker.SetActive(pinned);
        bool notOnMap = data != null && data.IsLocked && MissingOnKnownMap(data.linkedAchievement).Count > 0;
        if (notOnMap && _notOnMap == null && Plugin.Hud.Style is { } crossStyle)
            _notOnMap = CreateMarker(crossStyle, "NotOnMap", new Vector2(1f, 0f), NotOnMapOffset, HudStyle.Unattainable, crossStyle.Cross);
        if (_notOnMap != null)
            _notOnMap.SetActive(notOnMap);
        bool suggested = data != null && Suggestions(_badge.manager).Contains(data.linkedAchievement);
        if (suggested && _suggested == null && Plugin.Hud.Style is { } starStyle)
            _suggested = CreateMarker(starStyle, "Suggested", Vector2.up, SuggestedOffset, HudStyle.ProgressFill, starStyle.Star);
        if (_suggested != null)
            _suggested.SetActive(suggested);
        bool conflicts = data != null && data.IsLocked && !pinned && ConflictOf(data.linkedAchievement) is not null;
        _fade.alpha = conflicts ? ConflictAlpha : 1f;
    }

    private GameObject CreateMarker(HudStyle style, string name, Vector2 corner, Vector2 offset, Color fill, Sprite glyphSprite)
    {
        GameObject marker = UiFactory.Create(name, transform);
        UiFactory.PinToCorner(marker, corner, offset, MarkerSize);
        UiFactory.AddImage(marker, style.Circle, fill);
        GameObject glyph = UiFactory.Create("Glyph", marker.transform);
        UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, MarkerGlyphSize);
        UiFactory.AddImage(glyph, glyphSprite, HudStyle.PinMarkerInk).preserveAspect = true;
        return marker;
    }
}

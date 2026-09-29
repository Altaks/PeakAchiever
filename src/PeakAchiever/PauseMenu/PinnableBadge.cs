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
    // Mockup: a 22 px marker overlapping the badge's top-right edge by 4 px.
    private const float MarkerSize = 22f;
    private const float MarkerGlyphSize = 12f;
    private static readonly Vector2 MarkerOffset = new(4f, 4f);
    // Faded enough to read as unavailable next to the other locked badges, still legible.
    private const float ConflictAlpha = 0.35f;

    private BadgeUI _badge = null!;
    private GameObject? _marker;
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
            case PinToggleOutcome.RejectedAlreadyEarned:
                Plugin.Hud.ShowToast(ModText.Get(ModTextKey.RefusalAlreadyEarned));
                break;
            case PinToggleOutcome.RejectedConflict:
                ACHIEVEMENTTYPE conflict = ConflictOf(data.linkedAchievement)!.Value;
                Plugin.Hud.ShowToast(ModText.Format(ModTextKey.RefusalConflict, BadgeCatalog.Present(conflict).Name));
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(outcome), outcome, "Unhandled pin outcome.");
        }
    }

    /// <summary>The pin marker on a pinned badge; a fade on one that conflicts with a pin.</summary>
    private void ShowPinState()
    {
        BadgeData? data = _badge.data;
        bool pinned = data != null && Plugin.Pins.Board.IsPinned(data.linkedAchievement);
        if (pinned && _marker == null && Plugin.Hud.Style is { } style)
            _marker = CreateMarker(style);
        if (_marker != null)
            _marker.SetActive(pinned);
        bool conflicts = data != null && data.IsLocked && !pinned && ConflictOf(data.linkedAchievement) is not null;
        _fade.alpha = conflicts ? ConflictAlpha : 1f;
    }

    private GameObject CreateMarker(HudStyle style)
    {
        GameObject marker = UiFactory.Create("PinMarker", transform);
        UiFactory.PinToCorner(marker, Vector2.one, MarkerOffset, MarkerSize);
        UiFactory.AddImage(marker, style.Circle, HudStyle.ProgressFill);
        GameObject glyph = UiFactory.Create("Glyph", marker.transform);
        UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, MarkerGlyphSize);
        UiFactory.AddImage(glyph, style.Pin, HudStyle.PinMarkerInk).preserveAspect = true;
        return marker;
    }
}

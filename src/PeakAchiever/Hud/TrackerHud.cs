using System.Collections.Generic;
using BepInEx.Configuration;
using PeakAchiever.Game;
using PeakAchiever.Localization;
using PeakAchiever.Pinning;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;

namespace PeakAchiever.Hud;

/// <summary>
/// The mod's overlay: the pinned badge cards in the top-right corner during a run,
/// and the short confirmation or refusal line shown over the pause menu.
/// </summary>
internal sealed class TrackerHud : MonoBehaviour
{
    // Clean-run conditions on elapsed time and headcount change without any game event.
    private const float PeriodicRefreshSeconds = 1f;
    private const float ToastSeconds = 4f;
    // Above the game's own canvases, so the toast shows over the pause menu.
    private const int CanvasSortingOrder = 1000;
    private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);
    private const float ScreenMargin = 30f;
    private const float CardGap = 8f;
    private const float ToastBottomOffset = 120f;
    private const float ToastWidth = 640f;
    private const float ToastFontSize = 16f;
    private const float BannerFontSize = 13f;
    private const int ToastPadding = 12;
    private static readonly Vector2 ToastOutline = new(1.5f, -1.5f);

    private readonly List<BadgeCard> _cards = [];
    private PinnedBadgesStore _store = null!;
    private ConfigEntry<KeyboardShortcut> _toggleKey = null!;
    private HudStyle? _style;
    private GameObject _panel = null!;
    private GameObject _banner = null!;
    private GameObject _toast = null!;
    private TextMeshProUGUI _toastText = null!;
    private bool _refreshRequested = true;
    private bool _hiddenByPlayer;
    private float _nextPeriodicRefresh;
    private float _toastHideTime;

    /// <summary>Null until the game's GUI has loaded (see <see cref="Update"/>).</summary>
    public HudStyle? Style => _style;

    public void Init(PinnedBadgesStore store, ConfigEntry<KeyboardShortcut> toggleKey)
    {
        _store = store;
        _toggleKey = toggleKey;
    }

    /// <summary>Called by the game hooks whenever a counter, a stat, the segment or the pins change.</summary>
    public void RequestRefresh() => _refreshRequested = true;

    public void ShowToast(string message)
    {
        if (_style == null)
        {
            Plugin.Log.LogWarning($"Toast shown before the overlay was built: {message}");
            return;
        }
        _toastText.text = message;
        _toast.SetActive(true);
        _toastHideTime = Time.unscaledTime + ToastSeconds;
    }

    private void Update()
    {
        if (_toggleKey.Value.IsDown())
            _hiddenByPlayer = !_hiddenByPlayer;
        if (_style == null)
        {
            // The game's fonts and badge data only exist once its GUI is up.
            if (GUIManager.instance == null)
                return;
            _style = new HudStyle(Plugin.Log);
            Build(_style);
        }

        if (_toast.activeSelf && Time.unscaledTime >= _toastHideTime)
            _toast.SetActive(false);

        bool showCards = !_hiddenByPlayer && _store.Board.Pins.Count > 0 && RunFactsReader.IsInRun && !GUIManager.InPauseMenu;
        _panel.SetActive(showCards);
        if (!showCards || (!_refreshRequested && Time.unscaledTime < _nextPeriodicRefresh))
            return;
        Refresh(_style);
    }

    private void Refresh(HudStyle style)
    {
        _refreshRequested = false;
        _nextPeriodicRefresh = Time.unscaledTime + PeriodicRefreshSeconds;
        _banner.SetActive(RunSettings.blockingAchievements);

        RunFacts facts = RunFactsReader.Read();
        AchievementManager achievements = Singleton<AchievementManager>.Instance;
        IReadOnlyList<ACHIEVEMENTTYPE> pins = _store.Board.Pins;
        while (_cards.Count < pins.Count)
            _cards.Add(new BadgeCard(_panel.transform, style));
        for (int i = 0; i < _cards.Count; i++)
        {
            bool used = i < pins.Count;
            _cards[i].Root.SetActive(used);
            if (!used)
                continue;
            ACHIEVEMENTTYPE badge = pins[i];
            TrackedStatus status = BadgeRules.For(badge).Evaluate(facts, achievements.IsAchievementUnlocked(badge));
            _cards[i].Show(BadgeCatalog.Present(badge), status);
        }
    }

    private void Build(HudStyle style)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        _panel = UiFactory.Create("PinnedBadges", transform);
        var panelRect = (RectTransform)_panel.transform;
        panelRect.anchorMin = Vector2.one;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = Vector2.one;
        panelRect.anchoredPosition = new Vector2(-ScreenMargin, -ScreenMargin);
        VerticalLayoutGroup stack = _panel.AddComponent<VerticalLayoutGroup>();
        stack.spacing = CardGap;
        stack.childAlignment = TextAnchor.UpperRight;
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = false;
        stack.childForceExpandHeight = false;
        ContentSizeFitter fitter = _panel.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _banner = UiFactory.Create("AchievementsDisabled", _panel.transform);
        UiFactory.AddImage(_banner, style.RoundedRect, HudStyle.CardBackground).type = Image.Type.Sliced;
        _banner.AddComponent<HorizontalLayoutGroup>().padding = new RectOffset(ToastPadding, ToastPadding, 8, 8);
        UiFactory.SetPreferredSize(_banner, BadgeCard.Width, -1f);
        TextMeshProUGUI bannerText = UiFactory.AddText(_banner.transform, "Text", style.StrongFont, BannerFontSize, HudStyle.Unattainable);
        bannerText.text = ModText.Get(ModTextKey.AchievementsDisabled);

        _toast = UiFactory.Create("Toast", transform);
        var toastRect = (RectTransform)_toast.transform;
        toastRect.anchorMin = new Vector2(0.5f, 0f);
        toastRect.anchorMax = new Vector2(0.5f, 0f);
        toastRect.pivot = new Vector2(0.5f, 0f);
        toastRect.anchoredPosition = new Vector2(0f, ToastBottomOffset);
        UiFactory.AddImage(_toast, style.RoundedRect, HudStyle.CardBackground).type = Image.Type.Sliced;
        Outline border = _toast.AddComponent<Outline>();
        border.effectColor = HudStyle.Unattainable;
        border.effectDistance = ToastOutline;
        _toast.AddComponent<HorizontalLayoutGroup>().padding = new RectOffset(ToastPadding, ToastPadding, ToastPadding, ToastPadding);
        ContentSizeFitter toastFitter = _toast.AddComponent<ContentSizeFitter>();
        toastFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        toastFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        UiFactory.SetPreferredSize(_toast, ToastWidth, -1f);
        _toastText = UiFactory.AddText(_toast.transform, "Text", style.BodyFont, ToastFontSize, HudStyle.Ink);
        _toastText.alignment = TextAlignmentOptions.Center;
        _toast.SetActive(false);
    }
}

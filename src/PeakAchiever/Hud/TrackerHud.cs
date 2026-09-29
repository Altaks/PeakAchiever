using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Controls;
using PeakAchiever.Game;
using PeakAchiever.Localization;
using PeakAchiever.PauseMenu;
using PeakAchiever.Team;
using PeakAchiever.Tracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // Cards of a lost run tear one after the other, this far apart.
    private const float TearStaggerSeconds = 0.18f;

    private readonly Dictionary<ACHIEVEMENTTYPE, CardSlot> _slots = [];
    private readonly TornCards _tornCards = new();
    // Torn cards in the order they tore; they sit at the end of the column in that order.
    private readonly List<ACHIEVEMENTTYPE> _tornOrder = [];
    private PinnedBadgeTracker _tracker = null!;
    private TrackerToggleKey _toggleKey = null!;
    private HudStyle? _style;
    private StatsPanel? _stats;
    private GameObject _panel = null!;
    private GameObject _banner = null!;
    private GameObject _toast = null!;
    private TextMeshProUGUI _toastText = null!;
    private bool _refreshRequested = true;
    private bool _cardsStale = true;
    private bool _hiddenByPlayer;
    private float _nextPeriodicRefresh;
    private float _toastHideTime;

    /// <summary>Null until the game's GUI has loaded (see <see cref="Update"/>).</summary>
    public HudStyle? Style => _style;

    /// <summary>Null until the game's GUI has loaded, like <see cref="Style"/>.</summary>
    public StatsPanel? Stats => _stats;

    public void Init(PinnedBadgeTracker tracker, TrackerToggleKey toggleKey)
    {
        _tracker = tracker;
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
        if (_toggleKey.Action.WasPressedThisFrame())
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
        _stats!.Tick();

        // Evaluated even while the cards are hidden: the inventory marks depend on it too.
        if (_refreshRequested || Time.unscaledTime >= _nextPeriodicRefresh)
            Evaluate();

        bool showCards = !_hiddenByPlayer && _tracker.Tracked.Count > 0 && !GUIManager.InPauseMenu;
        _panel.SetActive(showCards);
        if (showCards)
            ShowCards(_style);
    }

    private void Evaluate()
    {
        _refreshRequested = false;
        _nextPeriodicRefresh = Time.unscaledTime + PeriodicRefreshSeconds;
        ItemTraits forbiddenBefore = _tracker.ForbiddenItems;
        RunFacts? facts = RunFactsReader.IsInRun ? RunFactsReader.Read(Plugin.Splits.History) : null;
        // Recorded whatever is pinned, so the ETA has past runs to go by once Speed Climber is.
        if (facts != null)
            Plugin.Splits.RecordFinished(facts.BiomeSplits, runEnded: false);
        // Kept current for the team, on the same cadence as the tracker.
        TeamSync.PublishEarned();
        _tracker.Evaluate(facts);
        ScheduleTears();
        _cardsStale = true;
        // The inventory marks are drawn when the game fills its slots, so have it refill them.
        if (_tracker.ForbiddenItems != forbiddenBefore)
            GUIManager.instance.UpdateItems();
    }

    private void ShowCards(HudStyle style)
    {
        if (!_cardsStale)
            return;
        _cardsStale = false;
        _banner.SetActive(RunSettings.blockingAchievements);
        IReadOnlyList<TrackedBadge> tracked = _tracker.Tracked;
        foreach (CardSlot slot in _slots.Values)
            slot.Root.SetActive(false);
        // The banner stays first.
        int place = _banner.transform.GetSiblingIndex() + 1;
        foreach (TrackedBadge badge in TornCards.Arrange(tracked, _tornOrder))
        {
            if (!_slots.TryGetValue(badge.Badge, out CardSlot slot))
                _slots[badge.Badge] = slot = new CardSlot(_panel.transform, style);
            slot.Root.SetActive(true);
            slot.Root.transform.SetSiblingIndex(place++);
            if (slot.Torn != _tornOrder.Contains(badge.Badge))
                slot.SetTorn(!slot.Torn);
            slot.Show(BadgeCatalog.Present(badge.Badge), badge.Status, badge.Detail);
        }
    }

    /// <summary>
    /// Keeps the torn list in step with the statuses: a card already impossible when first seen goes
    /// torn at once; one that just turned impossible tears on screen, after the ones before it.
    /// </summary>
    private void ScheduleTears()
    {
        IReadOnlyList<TrackedBadge> tracked = _tracker.Tracked;
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = _tornCards.Update(tracked);
        _tornOrder.RemoveAll(torn => !tracked.Any(badge => badge.Badge == torn && badge.Status is TrackedStatus.Unattainable));
        foreach (TrackedBadge badge in tracked)
        {
            if (badge.Status is TrackedStatus.Unattainable && !_tornOrder.Contains(badge.Badge) && !tearing.Contains(badge.Badge))
                _tornOrder.Add(badge.Badge);
        }
        for (int i = 0; i < tearing.Count; i++)
            StartCoroutine(TearLater(tearing[i], i * TearStaggerSeconds));
    }

    private IEnumerator TearLater(ACHIEVEMENTTYPE badge, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);
        if (_tornOrder.Contains(badge) || _style == null)
            yield break;
        // Hidden, it simply goes torn and last; on screen, every card moves from where it was.
        if (!_panel.activeInHierarchy || !_slots.TryGetValue(badge, out CardSlot torn))
        {
            _tornOrder.Add(badge);
            _cardsStale = true;
            yield break;
        }
        Dictionary<CardSlot, float> before = _slots.Values.Where(slot => slot.Root.activeSelf).ToDictionary(slot => slot, Height);
        _tornOrder.Add(badge);
        _cardsStale = true;
        ShowCards(_style);
        torn.SetTorn(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_panel.transform);
        foreach (KeyValuePair<CardSlot, float> slot in before)
        {
            float shift = slot.Value - Height(slot.Key);
            if (slot.Key == torn)
                slot.Key.Motion.TearAndFall(shift);
            else if (shift != 0f)
                slot.Key.Motion.HoldThenSlide(shift);
        }
    }

    // In the column's own units, those the slot's body is shifted in.
    private static float Height(CardSlot slot) => slot.Root.transform.localPosition.y;

    private void Build(HudStyle style)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;
        _stats = new StatsPanel(Plugin.Splits);

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

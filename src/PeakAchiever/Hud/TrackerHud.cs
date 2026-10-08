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
    private const float GroupFontSize = 10f;
    private const float GroupCharacterSpacing = 6f;
    private static readonly Vector2 GroupShadow = new(1f, -1f);
    private const int ToastPadding = 12;
    private static readonly Vector2 ToastOutline = new(1.5f, -1.5f);

    // Cards of a lost run tear one after the other, this far apart.
    private const float TearStaggerSeconds = 0.18f;
    // A torn card folds to one line once its tear and fall have played (SlotMotion: rip, then a fall of
    // at most this long).
    private const float TearSettleSeconds = 1.3f;

    private readonly Dictionary<ACHIEVEMENTTYPE, CardSlot> _slots = [];
    private readonly TornCards _tornCards = new();
    // Torn cards in the order they tore; they sit at the end of the column in that order.
    private readonly List<ACHIEVEMENTTYPE> _tornOrder = [];
    // When each torn card tore on screen; one torn before it was seen has no entry and shows folded.
    private readonly Dictionary<ACHIEVEMENTTYPE, float> _toreAt = [];
    private readonly FoldedCards _foldedCards = new();
    private IReadOnlyCollection<ACHIEVEMENTTYPE> _folded = [];
    private PinnedBadgeTracker _tracker = null!;
    private TrackerToggleKey _toggleKey = null!;
    private HudStyle? _style;
    private StatsPanel? _stats;
    private TeamPanel? _team;
    private GameObject _panel = null!;
    private OnScreenMarkers _markers = null!;
    private GameObject _banner = null!;
    private GameObject _teamHeader = null!;
    private GameObject _ownHeader = null!;
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

    /// <summary>Null until the game's GUI has loaded, like <see cref="Style"/>.</summary>
    public TeamPanel? Team => _team;

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
        _team!.Tick();

        // Evaluated even while the cards are hidden: the inventory marks depend on it too.
        if (_refreshRequested || Time.unscaledTime >= _nextPeriodicRefresh)
            Evaluate();

        bool showCards = !_hiddenByPlayer && _tracker.Tracked.Count > 0 && !GUIManager.InPauseMenu;
        _panel.SetActive(showCards);
        if (showCards)
            ShowCards(_style);
        PointLocators(showCards);
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
        _folded = _foldedCards.Update(_tracker.Tracked, Time.unscaledTime);
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
        // The banner stays first; the host's team pins then come as a group above the player's own.
        int place = _banner.transform.GetSiblingIndex() + 1;
        TrackedBadge[] team = tracked.Where(badge => badge.ForTeam).ToArray();
        TrackedBadge[] own = tracked.Where(badge => !badge.ForTeam).ToArray();
        _teamHeader.SetActive(team.Length > 0);
        _ownHeader.SetActive(team.Length > 0 && own.Length > 0);
        _teamHeader.transform.SetSiblingIndex(place++);
        foreach (TrackedBadge badge in TornCards.Arrange(team, _tornOrder, _folded))
            ShowSlot(style, badge, ref place);
        _ownHeader.transform.SetSiblingIndex(place++);
        foreach (TrackedBadge badge in TornCards.Arrange(own, _tornOrder, _folded))
            ShowSlot(style, badge, ref place);
    }

    private void ShowSlot(HudStyle style, TrackedBadge badge, ref int place)
    {
        if (!_slots.TryGetValue(badge.Badge, out CardSlot slot))
            _slots[badge.Badge] = slot = new CardSlot(_panel.transform, style);
        slot.Root.SetActive(true);
        slot.Root.transform.SetSiblingIndex(place++);
        bool torn = _tornOrder.Contains(badge.Badge);
        if (slot.Torn != torn)
            slot.SetTorn(torn);
        bool settled = !_toreAt.TryGetValue(badge.Badge, out float toreAt) || Time.unscaledTime >= toreAt + TearSettleSeconds;
        slot.Show(BadgeCatalog.Present(badge.Badge), badge, compact: _folded.Contains(badge.Badge) || (torn && settled));
    }

    // A group's title over its cards, in the column's own right-aligned flow.
    private GameObject GroupHeader(HudStyle style, string name, ModTextKey title)
    {
        TextMeshProUGUI text = UiFactory.AddText(_panel.transform, name, style.StrongFont, GroupFontSize, HudStyle.Ink);
        text.text = ModText.Get(title);
        text.characterSpacing = GroupCharacterSpacing;
        text.alignment = TextAlignmentOptions.Left;
        UiFactory.SetPreferredSize(text.gameObject, BadgeCard.Width, -1f);
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectDistance = GroupShadow;
        text.gameObject.SetActive(false);
        return text.gameObject;
    }

    /// <summary>
    /// Every frame, turns each in-play card's locator towards its target, and places the on-screen markers
    /// (when the setting is on). Hidden outside a run, while the cards are, and with nothing to point at.
    /// </summary>
    private void PointLocators(bool showCards)
    {
        _markers.Begin();
        // == null, not ?. or is: Unity objects override the null check, which the compiler's flow analysis
        // does not follow, hence the ! once checked.
        Camera camera = Camera.main;
        Character scout = Character.localCharacter;
        bool canPoint = showCards && camera != null && scout != null;
        foreach (TrackedBadge badge in _tracker.Tracked)
        {
            if (!_slots.TryGetValue(badge.Badge, out CardSlot slot))
                continue;
            bool inPlay = badge.Status is TrackedStatus.Attainable or TrackedStatus.Holding;
            Vector3? target = null;
            LocatorTarget? kind = Locators.TargetOf(badge.Badge);
            if (canPoint && inPlay && kind != null)
                target = Locators.Find(kind.Value, scout!.Center);
            if (target is not { } position)
            {
                slot.Front.ShowLocator(null, 0f);
                continue;
            }
            Vector3 toTarget = position - scout!.Center;
            float bearing = Vector3.SignedAngle(Flat(camera!.transform.forward), Flat(toTarget), Vector3.up);
            slot.Front.ShowLocator(bearing, toTarget.magnitude);
            if (Plugin.ShowMarkers.Value)
                _markers.Place(camera, position, Locators.NameOf(kind!.Value), toTarget.magnitude);
        }
        _markers.End();
    }

    // On the ground plane: the arrow turns like a compass, whatever the camera's pitch.
    private static Vector3 Flat(Vector3 direction) => new(direction.x, 0f, direction.z);

    /// <summary>
    /// Keeps the torn list in step with the statuses: a card already impossible when first seen goes
    /// torn at once; one that just turned impossible tears on screen, after the ones before it.
    /// </summary>
    private void ScheduleTears()
    {
        IReadOnlyList<TrackedBadge> tracked = _tracker.Tracked;
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = _tornCards.Update(tracked);
        _tornOrder.RemoveAll(torn => !tracked.Any(badge => badge.Badge == torn && badge.Status is TrackedStatus.Unattainable));
        foreach (ACHIEVEMENTTYPE gone in _toreAt.Keys.Where(badge => !_tornOrder.Contains(badge)).ToArray())
            _toreAt.Remove(gone);
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
        _toreAt[badge] = Time.unscaledTime;
        StartCoroutine(RefreshWhenSettled());
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

    // Folds the torn card once its tear has played.
    private IEnumerator RefreshWhenSettled()
    {
        yield return new WaitForSecondsRealtime(TearSettleSeconds);
        _cardsStale = true;
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
        _team = new TeamPanel();

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
        _teamHeader = GroupHeader(style, "TeamPins", ModTextKey.GroupTeam);
        _ownHeader = GroupHeader(style, "OwnPins", ModTextKey.GroupOwn);

        _markers = new OnScreenMarkers(transform, style);

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

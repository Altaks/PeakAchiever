using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PeakAchiever.Hud;

/// <summary>
/// A card's bar drawn like one of PEAK's stamina bar segments: the affliction's own hatched fill, outline
/// and shadow, copied from the game's bar (StaminaBar.afflictions, v2.4.c), on the bar's backing, without
/// the segment's icon. Near its limit the outline blinks, so the affliction's colour stays readable.
/// </summary>
internal sealed class AfflictionBar
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float Height = 16f;
    // Keeps the bar readable when the figures beside it are long ("0:42:13 / 1:00:00").
    private const float MinTrackWidth = 40f;
    // Below this share the game hides a segment (BarAffliction.ChangeAffliction).
    private const float HiddenBelow = 0.01f;
    private const string IconName = "Icon";
    private const string OutlineName = "Outline";

    private readonly Dictionary<CharacterAfflictions.STATUSTYPE, Segment> _segments = [];
    private readonly RectTransform _track;
    private bool _warnedMissing;

    public AfflictionBar(Transform parent)
    {
        Root = UiFactory.Create("AfflictionBar", parent);
        LayoutElement rootLayout = Root.AddComponent<LayoutElement>();
        rootLayout.flexibleWidth = 1f;
        rootLayout.minWidth = MinTrackWidth;
        rootLayout.preferredHeight = Height;

        GameObject track = UiFactory.Create("Track", Root.transform);
        _track = (RectTransform)track.transform;
        _track.anchorMin = Vector2.zero;
        _track.anchorMax = Vector2.one;
        _track.offsetMin = Vector2.zero;
        _track.offsetMax = Vector2.zero;
        Image backing = GUIManager.instance.bar.backing;
        Image trackImage = track.AddComponent<Image>();
        trackImage.sprite = backing.sprite;
        trackImage.type = backing.type;
        trackImage.material = backing.material;
        trackImage.pixelsPerUnitMultiplier = backing.pixelsPerUnitMultiplier;
        trackImage.color = backing.color;
        trackImage.raycastTarget = false;
        Root.SetActive(false);
    }

    public GameObject Root { get; }

    /// <summary>The affliction a progress is drawn as; null keeps the card's yellow bar.</summary>
    public static CharacterAfflictions.STATUSTYPE? LookOf(Progress progress) =>
        // The run time has no affliction of its own: it takes the arrow's, the bar's neutral colour.
        progress.Affliction ?? (progress.Unit == ProgressUnit.Duration ? CharacterAfflictions.STATUSTYPE.Arrow : null);

    /// <returns>False when the game's bar has no segment of that affliction; the card then keeps its own bar.</returns>
    public bool Show(CharacterAfflictions.STATUSTYPE look, float share, bool nearLimit)
    {
        if (SegmentOf(look) is not { } shown)
        {
            Root.SetActive(false);
            return false;
        }
        foreach (Segment segment in _segments.Values)
            segment.Root.SetActive(segment == shown && share >= HiddenBelow);
        shown.Rect.anchorMax = new Vector2(Mathf.Clamp01(share), 1f);
        shown.Blink.enabled = nearLimit;
        Root.SetActive(true);
        return true;
    }

    public void Hide() => Root.SetActive(false);

    private Segment? SegmentOf(CharacterAfflictions.STATUSTYPE look)
    {
        if (_segments.TryGetValue(look, out Segment segment))
            return segment;
        BarAffliction? native = GUIManager.instance.bar.afflictions.FirstOrDefault(bar => bar.afflictionType == look);
        if (native == null)
        {
            if (!_warnedMissing)
                Plugin.Log.LogWarning($"The stamina bar has no {look} segment to copy; the card keeps its yellow bar.");
            _warnedMissing = true;
            return null;
        }
        segment = Copy(native);
        _segments[look] = segment;
        return segment;
    }

    private Segment Copy(BarAffliction native)
    {
        GameObject copy = Object.Instantiate(native.gameObject, _track);
        copy.name = native.afflictionType.ToString();
        // The copy is sized by its share of the track, not by the game's bar logic or its fitter.
        Object.DestroyImmediate(copy.GetComponent<BarAffliction>());
        Object.DestroyImmediate(copy.GetComponent<ContentSizeFitter>());
        var rect = (RectTransform)copy.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        foreach (Image image in copy.GetComponentsInChildren<Image>(includeInactive: true))
            image.raycastTarget = false;
        // The segment's icon is left out: the card already has the badge's own.
        copy.transform.Find(IconName).gameObject.SetActive(false);
        Image outline = copy.transform.Find(OutlineName).GetComponent<Image>();
        OutlineBlink blink = outline.gameObject.AddComponent<OutlineBlink>();
        blink.enabled = false;
        copy.SetActive(true);
        return new Segment(copy, rect, blink);
    }

    private sealed record Segment(GameObject Root, RectTransform Rect, OutlineBlink Blink);
}

/// <summary>Pulses an outline's opacity while enabled; restores it when disabled.</summary>
internal sealed class OutlineBlink : MonoBehaviour
{
    // Two pulses a second, between a faint and a full outline.
    private const float PulsesPerSecond = 2f;
    private const float FaintAlpha = 0.2f;

    private Image _outline = null!;
    private float _alpha;

    private void Awake()
    {
        _outline = GetComponent<Image>();
        _alpha = _outline.color.a;
    }

    private void Update()
    {
        float wave = (Mathf.Sin(Time.unscaledTime * PulsesPerSecond * 2f * Mathf.PI) + 1f) / 2f;
        Color color = _outline.color;
        color.a = Mathf.Lerp(FaintAlpha, _alpha, wave);
        _outline.color = color;
    }

    private void OnDisable()
    {
        if (_outline == null)
            return;
        Color color = _outline.color;
        color.a = _alpha;
        _outline.color = color;
    }
}

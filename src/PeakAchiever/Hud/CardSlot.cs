using System.Collections;
using PeakAchiever.Game;
using PeakAchiever.Tracking;
using UnityEngine;
using UnityEngine.UI;

namespace PeakAchiever.Hud;

/// <summary>
/// One place in the tracker's column. It holds the badge's card twice, each behind one half of a jagged
/// tear: while intact only the front card shows, unmasked; once torn both halves show, a few pixels apart
/// and slightly turned. The slot's body can be shifted and turned to animate a tear, a fall or a slide.
/// </summary>
internal sealed class CardSlot
{
    // The torn halves at rest: pushed apart and turned away from the tear.
    private const float RestShift = 4f;
    private const float RestTilt = 1.6f;

    private readonly BadgeCard _front;
    private readonly BadgeCard _back;
    private readonly Image _frontMaskImage;
    private readonly Mask _frontMask;
    private readonly RectTransform _frontHalf;
    private readonly RectTransform _backHalf;
    private readonly LayoutElement _layout;

    public CardSlot(Transform column, HudStyle style)
    {
        Root = UiFactory.Create("CardSlot", column);
        _layout = Root.AddComponent<LayoutElement>();
        _layout.preferredWidth = BadgeCard.Width;
        Motion = Root.AddComponent<SlotMotion>();
        Body = (RectTransform)UiFactory.Create("Body", Root.transform).transform;
        Stretch(Body, new Vector2(0.5f, 0.5f));

        _frontHalf = Half("Front", style.TearLeft, new Vector2(0f, 0f), out _frontMaskImage, out _frontMask);
        _backHalf = Half("Back", style.TearRight, new Vector2(1f, 0f), out _, out _);
        _front = Card(_frontHalf, style);
        _back = Card(_backHalf, style);
        SetTorn(false);
        Motion.Init(this);
    }

    public GameObject Root { get; }

    /// <summary>What the animations move; the layout places <see cref="Root"/>.</summary>
    public RectTransform Body { get; }

    public SlotMotion Motion { get; }

    public bool Torn { get; private set; }

    public void Show(BadgePresentation badge, TrackedStatus status, BadgeDetail? detail)
    {
        _front.Show(badge, status, detail);
        if (Torn)
            _back.Show(badge, status, detail);
        var card = (RectTransform)_front.Root.transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
        float height = LayoutUtility.GetPreferredHeight(card);
        foreach (BadgeCard shown in new[] { _front, _back })
        {
            var rect = (RectTransform)shown.Root.transform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }
        _layout.preferredHeight = height;
    }

    /// <summary>Shows the card whole, or as two halves at rest; the halves' pose is then the animations' to change.</summary>
    public void SetTorn(bool torn)
    {
        Torn = torn;
        _frontMaskImage.enabled = torn;
        _frontMask.enabled = torn;
        _backHalf.gameObject.SetActive(torn);
        SetTear(torn ? 1f : 0f);
    }

    /// <summary>The halves' pose between whole (0) and torn at rest (1).</summary>
    public void SetTear(float open)
    {
        _frontHalf.anchoredPosition = new Vector2(-RestShift * open, 0f);
        _frontHalf.localEulerAngles = new Vector3(0f, 0f, RestTilt * open);
        _backHalf.anchoredPosition = new Vector2(RestShift * open, 0f);
        _backHalf.localEulerAngles = new Vector3(0f, 0f, -RestTilt * open);
    }

    private RectTransform Half(string name, Sprite mask, Vector2 pivot, out Image image, out Mask component)
    {
        var half = (RectTransform)UiFactory.Create(name, Body).transform;
        Stretch(half, pivot);
        image = half.gameObject.AddComponent<Image>();
        image.sprite = mask;
        image.raycastTarget = false;
        component = half.gameObject.AddComponent<Mask>();
        component.showMaskGraphic = false;
        return half;
    }

    private static BadgeCard Card(RectTransform half, HudStyle style)
    {
        var card = new BadgeCard(half, style);
        var rect = (RectTransform)card.Root.transform;
        // Hung from the top of the slot, full width; its height is set to the card's own in Show.
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return card;
    }

    private static void Stretch(RectTransform rect, Vector2 pivot)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = pivot;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

/// <summary>Animates a slot: the tear opening where it stands, then the fall or slide to its new place.</summary>
internal sealed class SlotMotion : MonoBehaviour
{
    private const float RipSeconds = 0.3f;
    private const float SlideSeconds = 0.38f;
    private const float FallBaseSeconds = 0.42f;
    private const float FallMaxSeconds = 0.9f;
    // A longer drop takes longer, as a falling tag would.
    private const float FallSecondsPerPixel = 0.001f;
    // The fall ends turned a little, then settles through a small bounce.
    private const float FallTilt = 3f;
    private const float BounceTilt = -1f;
    private const float BounceHeight = 6f;
    private const float FallShare = 0.78f;
    private const float BounceShare = 0.9f;

    private CardSlot _slot = null!;

    public void Init(CardSlot slot) => _slot = slot;

    /// <summary>Opens the tear where the card stands, then drops it by <paramref name="drop"/> to its new place.</summary>
    public void TearAndFall(float drop)
    {
        StopAllCoroutines();
        StartCoroutine(TearThenFall(drop));
    }

    /// <summary>Waits while a torn card opens, then slides by <paramref name="shift"/> to the new place.</summary>
    public void HoldThenSlide(float shift)
    {
        StopAllCoroutines();
        StartCoroutine(Slide(shift));
    }

    private IEnumerator TearThenFall(float drop)
    {
        _slot.SetTorn(true);
        _slot.Body.anchoredPosition = new Vector2(0f, drop);
        _slot.Body.localEulerAngles = Vector3.zero;
        for (float t = 0f; t < RipSeconds; t += Time.unscaledDeltaTime)
        {
            _slot.SetTear(EaseOut(t / RipSeconds));
            yield return null;
        }
        _slot.SetTear(1f);
        float seconds = Mathf.Min(FallMaxSeconds, FallBaseSeconds + Mathf.Abs(drop) * FallSecondsPerPixel);
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            Pose(drop, t / seconds);
            yield return null;
        }
        Rest();
    }

    private IEnumerator Slide(float shift)
    {
        _slot.Body.anchoredPosition = new Vector2(0f, shift);
        _slot.Body.localEulerAngles = Vector3.zero;
        for (float t = 0f; t < RipSeconds; t += Time.unscaledDeltaTime)
            yield return null;
        for (float t = 0f; t < SlideSeconds; t += Time.unscaledDeltaTime)
        {
            _slot.Body.anchoredPosition = new Vector2(0f, shift * (1f - EaseOut(t / SlideSeconds)));
            yield return null;
        }
        Rest();
    }

    // Accelerates down, lands turned, bounces up a little, settles.
    private void Pose(float drop, float progress)
    {
        float y;
        float tilt;
        if (progress < FallShare)
        {
            float fall = progress / FallShare;
            y = drop * (1f - fall * fall);
            tilt = FallTilt * fall;
        }
        else if (progress < BounceShare)
        {
            float up = (progress - FallShare) / (BounceShare - FallShare);
            y = BounceHeight * EaseOut(up);
            tilt = Mathf.Lerp(FallTilt, BounceTilt, up);
        }
        else
        {
            float down = (progress - BounceShare) / (1f - BounceShare);
            y = BounceHeight * (1f - down * down);
            tilt = Mathf.Lerp(BounceTilt, 0f, down);
        }
        _slot.Body.anchoredPosition = new Vector2(0f, y);
        _slot.Body.localEulerAngles = new Vector3(0f, 0f, -tilt);
    }

    private void Rest()
    {
        _slot.Body.anchoredPosition = Vector2.zero;
        _slot.Body.localEulerAngles = Vector3.zero;
    }

    private void OnDisable()
    {
        if (_slot == null)
            return;
        StopAllCoroutines();
        Rest();
        if (_slot.Torn)
            _slot.SetTear(1f);
    }

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}

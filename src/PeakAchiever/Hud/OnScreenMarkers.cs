using System.Collections.Generic;
using PeakAchiever.Localization;
using TMPro;
using UnityEngine;

namespace PeakAchiever.Hud;

/// <summary>
/// A marker over each locator target in view: a small diamond and the target's name with its distance,
/// like a ping. Off when the "on-screen markers" setting is.
/// </summary>
internal sealed class OnScreenMarkers
{
    // Sizes in reference pixels of the 1920x1080 canvas, taken from the signed-off mockup.
    private const float DiamondSize = 14f;
    private const float DiamondRim = 2f;
    private const float LabelFontSize = 11f;
    private const float LabelOffset = 14f;
    private const float LabelWidth = 260f;
    // Above the target's origin, roughly at a scout's head height, so the marker clears the object.
    private const float WorldLift = 2f;

    private readonly Transform _canvas;
    private readonly HudStyle _style;
    private readonly List<Marker> _markers = [];
    private int _used;

    public OnScreenMarkers(Transform canvas, HudStyle style)
    {
        _canvas = canvas;
        _style = style;
    }

    /// <summary>Starts a frame: every marker is hidden until placed again by <see cref="Place"/>.</summary>
    public void Begin() => _used = 0;

    /// <summary>Places the next marker over <paramref name="world"/>; nothing when it is behind the camera.</summary>
    public void Place(Camera camera, Vector3 world, string name, float meters)
    {
        Vector3 screen = camera.WorldToScreenPoint(world + Vector3.up * WorldLift);
        if (screen.z <= 0f)
            return;
        if (_used == _markers.Count)
            _markers.Add(new Marker(_canvas, _style));
        Marker marker = _markers[_used++];
        marker.Root.SetActive(true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas, screen, null, out Vector2 local);
        marker.Rect.anchoredPosition = local;
        marker.Show(ModText.Format(ModTextKey.MarkerLabel, name.ToUpperInvariant(), Mathf.RoundToInt(meters)));
    }

    /// <summary>Ends a frame: the markers not placed in it hide.</summary>
    public void End()
    {
        for (int i = _used; i < _markers.Count; i++)
            _markers[i].Root.SetActive(false);
    }

    private sealed class Marker
    {
        private readonly TextMeshProUGUI _label;
        private string _shown = "";

        public Marker(Transform canvas, HudStyle style)
        {
            Root = UiFactory.Create("Marker", canvas);
            Rect = (RectTransform)Root.transform;
            Rect.anchorMin = Rect.anchorMax = new Vector2(0.5f, 0.5f);
            Rect.sizeDelta = Vector2.zero;
            // The diamond: a rim square turned 45 degrees, a dark square inside it.
            GameObject rim = UiFactory.Create("Diamond", Root.transform);
            UiFactory.PinToCorner(rim, new Vector2(0.5f, 0.5f), Vector2.zero, DiamondSize);
            rim.transform.localEulerAngles = new Vector3(0f, 0f, 45f);
            UiFactory.AddImage(rim, style.RoundedRect, HudStyle.ProgressFill);
            GameObject inside = UiFactory.Create("Inside", rim.transform);
            UiFactory.PinToCorner(inside, new Vector2(0.5f, 0.5f), Vector2.zero, DiamondSize - 2 * DiamondRim);
            UiFactory.AddImage(inside, style.RoundedRect, HudStyle.CardBackground);
            _label = UiFactory.AddText(Root.transform, "Label", style.StrongFont, LabelFontSize, HudStyle.Ink);
            _label.alignment = TextAlignmentOptions.Bottom;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            var labelRect = _label.rectTransform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(LabelWidth, LabelFontSize * 2);
            labelRect.anchoredPosition = new Vector2(0f, LabelOffset);
        }

        public GameObject Root { get; }

        public RectTransform Rect { get; }

        public void Show(string label)
        {
            if (label == _shown)
                return;
            _shown = label;
            _label.text = label;
        }
    }
}

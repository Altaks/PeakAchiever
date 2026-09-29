using PeakAchiever.Hud;
using PeakAchiever.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PeakAchiever.Controls;

/// <summary>
/// A row added to the game's Controls page to set the tracker key: click the key, press another, or
/// Escape to keep it. Mirrors the rebinding the game runs for its own actions (PauseMenuRebindKeyPage).
/// </summary>
internal sealed class TrackerKeyRow : MonoBehaviour
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float RowHeight = 48f;
    private const float LabelFontSize = 20f;
    private const int RowPadding = 12;

    private TrackerToggleKey _key = null!;
    private TextMeshProUGUI _keyLabel = null!;
    private InputActionRebindingExtensions.RebindingOperation? _rebinding;

    /// <summary>Adds the row once to the page's list of controls.</summary>
    public static void AttachTo(PauseMenuControlsPage page, TrackerToggleKey key)
    {
        if (page.controlsMenuButtonsParent.GetComponentInChildren<TrackerKeyRow>(includeInactive: true) != null)
            return;
        if (Plugin.Hud.Style is not { } style)
        {
            Plugin.Log.LogWarning("The Controls page opened before the overlay was built; the tracker key row is left out.");
            return;
        }
        GameObject root = UiFactory.Create("PeakAchiever.TrackerKey", page.controlsMenuButtonsParent);
        UiFactory.AddImage(root, style.RoundedRect, HudStyle.CardBackground).type = Image.Type.Sliced;
        HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(RowPadding, RowPadding, RowPadding / 2, RowPadding / 2);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        UiFactory.SetPreferredSize(root, -1f, RowHeight);

        TextMeshProUGUI label = UiFactory.AddText(root.transform, "Label", style.StrongFont, LabelFontSize, HudStyle.Ink);
        label.text = ModText.Get(ModTextKey.ControlsToggleTracker);
        label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        TrackerKeyRow row = root.AddComponent<TrackerKeyRow>();
        row._key = key;
        Button button = UiFactory.AddButton(root.transform, "Key", style, key.DisplayName, row.StartRebinding);
        row._keyLabel = button.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void StartRebinding()
    {
        if (_rebinding != null)
            return;
        InputAction action = _key.Action;
        action.Disable();
        _keyLabel.text = ModText.Get(ModTextKey.ControlsPressAKey);
        _rebinding = action
            .PerformInteractiveRebinding(0)
            .WithControlsExcluding("<Mouse>")
            .WithControlsExcluding("<Gamepad>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(_ => Finish(saved: true))
            .OnCancel(_ => Finish(saved: false))
            .Start();
    }

    private void Finish(bool saved)
    {
        _rebinding?.Dispose();
        _rebinding = null;
        _key.Action.Enable();
        _keyLabel.text = _key.DisplayName;
        if (!saved)
            return;
        _key.Save();
        Plugin.Hud.ShowToast(ModText.Format(ModTextKey.ControlsKeySet, _key.DisplayName));
    }

    // Leaving the page mid-way keeps the key it had.
    private void OnDisable()
    {
        if (_rebinding != null)
            _rebinding.Cancel();
    }
}

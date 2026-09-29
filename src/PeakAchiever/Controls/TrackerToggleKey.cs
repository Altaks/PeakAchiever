using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine.InputSystem;

namespace PeakAchiever.Controls;

/// <summary>The key that shows or hides the tracker: an Input System action kept in the config.</summary>
internal sealed class TrackerToggleKey
{
    private const string ActionName = "PeakAchiever.ToggleTracker";

    private readonly ConfigEntry<string> _entry;

    public TrackerToggleKey(ConfigFile config, ManualLogSource log)
    {
        _entry = config.Bind(
            "Tracker",
            "ToggleKey",
            ToggleKeyBinding.DefaultPath,
            "Shows or hides the tracker during a run. An Input System binding path such as <Keyboard>/f6; "
                + "set it from the game's Controls menu."
        );
        string? path = ToggleKeyBinding.FromConfig(_entry.Value);
        if (path == null)
        {
            log.LogWarning($"ToggleKey '{_entry.Value}' cannot be read (a key with modifiers?); using F6. Set it again from the Controls menu.");
            path = ToggleKeyBinding.DefaultPath;
        }
        // Rewrites a key saved by an older version in the new form.
        if (path != _entry.Value)
            _entry.Value = path;
        Action = new InputAction(ActionName, InputActionType.Button, path);
        Action.Enable();
    }

    public InputAction Action { get; }

    /// <summary>The key as the player reads it ("F6").</summary>
    public string DisplayName =>
        InputControlPath.ToHumanReadableString(Action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

    /// <summary>Keeps the key the player just picked.</summary>
    public void Save() => _entry.Value = Action.bindings[0].effectivePath;
}

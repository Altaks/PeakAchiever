using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine.InputSystem;

namespace PeakAchiever.Controls;

/// <summary>The key that shows or hides the tracker: an Input System action kept in the config.</summary>
internal sealed class TrackerToggleKey
{
    private const string ActionName = "PeakAchiever.ToggleTracker";
    // The binding group PauseMenuRebindKeyPage looks for to pick the keyboard binding (v2.4.c).
    private const string KeyboardMouseGroup = "Keyboard&Mouse";

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
        Action = new InputAction(ActionName, InputActionType.Button);
        Action.AddBinding(path).WithGroups(KeyboardMouseGroup);
        Action.Enable();
    }

    public InputAction Action { get; }

    public string Path => Action.bindings[0].effectivePath;

    public bool IsDefault => Path == ToggleKeyBinding.DefaultPath;

    /// <summary>The key as the player reads it ("F6").</summary>
    public string DisplayName => InputControlPath.ToHumanReadableString(Path, InputControlPath.HumanReadableStringOptions.OmitDevice);

    /// <summary>Keeps a key picked on the game's rebinding page.</summary>
    /// <returns>True when the key changed.</returns>
    public bool SaveIfChanged()
    {
        if (Path == _entry.Value)
            return false;
        _entry.Value = Path;
        return true;
    }

    public void ResetToDefault()
    {
        Action.ApplyBindingOverride(0, ToggleKeyBinding.DefaultPath);
        SaveIfChanged();
    }
}

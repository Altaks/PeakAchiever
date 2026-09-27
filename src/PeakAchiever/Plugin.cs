using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PeakAchiever.Hud;
using PeakAchiever.Pinning;
using UnityEngine;

namespace PeakAchiever;

/// <summary>
/// Pins badges from the pause menu and tracks, during the run, which of them can still be earned.
/// </summary>
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    internal static PinnedBadgesStore Pins { get; private set; } = null!;

    internal static TrackerHud Hud { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Pins = new PinnedBadgesStore(Config, Logger);
        ConfigEntry<KeyboardShortcut> toggleKey = Config.Bind(
            "Tracker",
            "ToggleKey",
            new KeyboardShortcut(KeyCode.F6),
            "Shows or hides the tracker during a run."
        );

        var overlay = new GameObject("PeakAchiever.Overlay");
        DontDestroyOnLoad(overlay);
        Hud = overlay.AddComponent<TrackerHud>();
        Hud.Init(Pins, toggleKey);

        new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}

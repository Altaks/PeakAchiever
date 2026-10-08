using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PeakAchiever.Controls;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Pinning;
using PeakAchiever.Settings;
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

    internal static PinnedBadgeTracker Tracker { get; private set; } = null!;

    internal static SplitHistoryStore Splits { get; private set; } = null!;

    internal static TrackerToggleKey ToggleKey { get; private set; } = null!;

    internal static TrackerHud Hud { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Pins = new PinnedBadgesStore(Config, Logger);
        Tracker = new PinnedBadgeTracker(Pins.Board);
        Splits = new SplitHistoryStore(Paths.ConfigPath, Logger);
        ToggleKey = new TrackerToggleKey(Config, Logger);

        var overlay = new GameObject("PeakAchiever.Overlay");
        DontDestroyOnLoad(overlay);
        Hud = overlay.AddComponent<TrackerHud>();
        Hud.Init(Tracker, ToggleKey);

        new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
        if (SettingsHandler.Instance != null)
            SettingsPatches.AddPinLimits(SettingsHandler.Instance);
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}

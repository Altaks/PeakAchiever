using HarmonyLib;

namespace PeakAchiever.Controls;

/// <summary>Adds the tracker key to the game's Controls page.</summary>
[HarmonyPatch]
internal static class ControlsPagePatches
{
    // PauseMenuControlsPage builds its rows in OnEnable (InitButtons, v2.4.c).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PauseMenuControlsPage), "OnEnable")]
    private static void AddTrackerKeyRow(PauseMenuControlsPage __instance) => TrackerKeyRow.AttachTo(__instance, Plugin.ToggleKey);
}

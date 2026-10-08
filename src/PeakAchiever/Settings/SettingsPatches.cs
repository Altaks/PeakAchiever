using HarmonyLib;
using PeakAchiever.Localization;

namespace PeakAchiever.Settings;

/// <summary>Adds the mod's settings to the game's, once the game has built its own.</summary>
[HarmonyPatch]
internal static class SettingsPatches
{
    private const string MaxPinsLabelKey = "PEAKACHIEVER_MAX_PINS";
    private const string MaxTeamPinsLabelKey = "PEAKACHIEVER_MAX_TEAM_PINS";

    /// <summary>
    /// For a handler the game built before the mod loaded; the hook below covers the others. Adds the limits
    /// once: the Settings menu lists every IExposedSetting of its tab, the mod's last.
    /// </summary>
    public static void AddModSettings(SettingsHandler handler)
    {
        if (handler.GetSetting<PinLimitSetting>() != null)
            return;
        handler.AddSetting(new PinLimitSetting(Plugin.Pins.Capacity, MaxPinsLabelKey, ModTextKey.SettingsMaxPins));
        handler.AddSetting(new PinLimitSetting(Plugin.Pins.TeamCapacity, MaxTeamPinsLabelKey, ModTextKey.SettingsMaxTeamPins));
        handler.AddSetting(new MarkersSetting(Plugin.ShowMarkers));
    }

    // GameHandler.Initialize builds the handler, which adds the game's settings in its constructor (v2.6.b).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SettingsHandler), MethodType.Constructor)]
    private static void AfterSettingsBuilt(SettingsHandler __instance) => AddModSettings(__instance);
}

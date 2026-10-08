using System.Collections.Generic;
using BepInEx.Configuration;
using PeakAchiever.Localization;
using UnityEngine.Localization;
using Zorro.Settings;

namespace PeakAchiever.Settings;

/// <summary>
/// Turns the on-screen markers over locator targets on or off, as an OFF / ON row of the game's Settings
/// menu, General tab, built like Hide room code (HideRoomCodeSetting: CustomLocalizedOffOnSetting, v2.6.b).
/// The value lives in the mod's BepInEx config.
/// </summary>
internal sealed class MarkersSetting : CustomLocalizedOffOnSetting, IExposedSetting
{
    private const string Category = "General";
    private const string LabelKey = "PEAKACHIEVER_MARKERS";

    private readonly ConfigEntry<bool> _entry;

    public MarkersSetting(ConfigEntry<bool> entry)
    {
        _entry = entry;
        // An edit of the config file shows the next time the tab opens.
        _entry.SettingChanged += (_, _) => Value = ToMode(_entry.Value);
    }

    /// <summary>Reads the config, not PlayerPrefs, so the file and the menu never disagree.</summary>
    public override void Load(ISettingsSaveLoad loader) => Value = ToMode(_entry.Value);

    public override void Save(ISettingsSaveLoad saver) => _entry.Value = Value == OffOnMode.ON;

    public override void ApplyValue() { }

    // CustomLocalizedOffOnSetting names its choices through the game's table; this list is unused (as in HideRoomCodeSetting).
    public override List<LocalizedString> GetLocalizedChoices() => null!;

    /// <remarks>Registered each time the tab shows, as for <see cref="PinLimitSetting"/>.</remarks>
    public string GetDisplayName()
    {
        GameTextTable.Register(LabelKey, ModTextKey.SettingsMarkers);
        return LabelKey;
    }

    public string GetCategory() => Category;

    protected override OffOnMode GetDefaultValue() => ToMode((bool)_entry.DefaultValue);

    private static OffOnMode ToMode(bool on) => on ? OffOnMode.ON : OffOnMode.OFF;
}

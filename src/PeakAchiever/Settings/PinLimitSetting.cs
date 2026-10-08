using BepInEx.Configuration;
using PeakAchiever.Localization;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace PeakAchiever.Settings;

/// <summary>
/// One of the mod's pin limits as a row of the game's Settings menu, General tab: the game builds it from
/// its own slider cell, as for Field of view (FovSetting: FloatSetting, IExposedSetting, v2.6.b). The value
/// lives in the mod's BepInEx config, not in the game's saved settings.
/// </summary>
internal sealed class PinLimitSetting : FloatSetting, IExposedSetting
{
    // SharedSettingsMenu.ShowSettings lists a setting under the tab whose SettingsCategory name it returns.
    private const string Category = "General";
    private const string WholeNumber = "F0";

    private readonly ConfigEntry<int> _entry;
    private readonly string _labelKey;
    private readonly ModTextKey _label;

    public PinLimitSetting(ConfigEntry<int> entry, string labelKey, ModTextKey label)
    {
        _entry = entry;
        _labelKey = labelKey;
        _label = label;
        // An edit of the config file shows the next time the tab opens.
        _entry.SettingChanged += (_, _) => Value = _entry.Value;
    }

    /// <summary>Reads the config, not PlayerPrefs, so the file and the menu never disagree.</summary>
    public override void Load(ISettingsSaveLoad loader)
    {
        float2 range = GetMinMaxValue();
        MinValue = range.x;
        MaxValue = range.y;
        Value = _entry.Value;
    }

    /// <summary>Writes the config; BepInEx saves the file and raises SettingChanged, which the pin board follows.</summary>
    public override void Save(ISettingsSaveLoad saver) => _entry.Value = Mathf.RoundToInt(Value);

    public override void ApplyValue() { }

    // The slider moves in fractions; a limit is a whole number of pins.
    public override float Clamp(float value) => Mathf.Round(base.Clamp(value));

    public override string Expose(float result) => Mathf.Round(result).ToString(WholeNumber);

    /// <remarks>
    /// SettingsUICell.Setup passes this to the row's LocalizedText as a key, each time the tab is shown; the
    /// label goes into the game's table right then, since the game rebuilds it when it reloads its texts.
    /// </remarks>
    public string GetDisplayName()
    {
        GameTextTable.Register(_labelKey, _label);
        return _labelKey;
    }

    public string GetCategory() => Category;

    protected override float GetDefaultValue() => (int)_entry.DefaultValue;

    // The entry's own bounds, so the config and the slider share one range.
    protected override float2 GetMinMaxValue()
    {
        var range = (AcceptableValueRange<int>)_entry.Description.AcceptableValues;
        return new float2(range.MinValue, range.MaxValue);
    }
}

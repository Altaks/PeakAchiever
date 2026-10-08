using BepInEx.Configuration;
using PeakAchiever.Settings;
using Zorro.Settings;

namespace PeakAchiever.Tests;

public sealed class MarkersSettingTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"MarkersSettingTests-{Guid.NewGuid():N}.cfg");
    private readonly ConfigEntry<bool> _entry;

    public MarkersSettingTests()
    {
        var config = new ConfigFile(_path, saveOnInit: false);
        _entry = config.Bind("Tracker", "ShowMarkers", true, "");
    }

    public void Dispose() => File.Delete(_path);

    [Fact]
    public void Loads_the_config_value()
    {
        // given
        _entry.Value = false;
        var setting = new MarkersSetting(_entry);

        // when: the loader is the game's PlayerPrefs store, which the setting never reads
        setting.Load(null!);

        // then
        Assert.Equal(OffOnMode.OFF, setting.Value);
    }

    [Fact]
    public void Saving_writes_the_config()
    {
        // given the row was switched off (EnumSetting.Value has a protected setter)
        var setting = new MarkersSetting(_entry);
        setting.Load(null!);
        typeof(EnumSetting<OffOnMode>).GetProperty(nameof(EnumSetting<OffOnMode>.Value))!.SetValue(setting, OffOnMode.OFF);

        // when
        setting.Save(null!);

        // then
        Assert.False(_entry.Value);
    }
}

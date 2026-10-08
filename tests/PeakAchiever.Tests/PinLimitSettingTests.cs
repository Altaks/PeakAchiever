using BepInEx.Configuration;
using PeakAchiever.Localization;
using PeakAchiever.Settings;
using Zorro.Settings;

namespace PeakAchiever.Tests;

public sealed class PinLimitSettingTests : IDisposable
{
    private const int Default = 5;
    private const int Min = 1;
    private const int Max = 12;

    private readonly string _path = Path.Combine(Path.GetTempPath(), $"PinLimitSettingTests-{Guid.NewGuid():N}.cfg");
    private readonly ConfigEntry<int> _entry;

    public PinLimitSettingTests()
    {
        var config = new ConfigFile(_path, saveOnInit: false);
        _entry = config.Bind("Tracker", "MaxPinnedBadges", Default, new ConfigDescription("", new AcceptableValueRange<int>(Min, Max)));
    }

    public void Dispose() => File.Delete(_path);

    private PinLimitSetting LoadedSetting()
    {
        var setting = new PinLimitSetting(_entry, "PEAKACHIEVER_TEST", ModTextKey.SettingsMaxPins);
        // The loader is the game's PlayerPrefs store, which the setting never reads.
        setting.Load(null!);
        return setting;
    }

    [Fact]
    public void Loads_the_config_value_and_its_range()
    {
        // given
        _entry.Value = 8;

        // when
        PinLimitSetting setting = LoadedSetting();

        // then
        Assert.Equal((8f, (float)Min, (float)Max), (setting.Value, setting.MinValue, setting.MaxValue));
    }

    [Theory]
    [InlineData(6.4f, 6f)]
    [InlineData(6.6f, 7f)]
    [InlineData(0f, Min)]
    [InlineData(40f, Max)]
    public void Clamps_to_a_whole_number_of_pins_in_range(float dragged, float expected)
    {
        // given
        PinLimitSetting setting = LoadedSetting();

        // when
        float clamped = setting.Clamp(dragged);

        // then
        Assert.Equal(expected, clamped);
    }

    [Fact]
    public void Shows_a_whole_number()
    {
        // given
        PinLimitSetting setting = LoadedSetting();

        // when
        string shown = setting.Expose(6.6f);

        // then
        Assert.Equal("7", shown);
    }

    [Fact]
    public void Saving_writes_the_value_to_the_config_as_a_whole_number()
    {
        // given the slider left the value between two whole numbers (FloatSetting.Value has a protected setter)
        PinLimitSetting setting = LoadedSetting();
        typeof(FloatSetting).GetProperty(nameof(FloatSetting.Value))!.SetValue(setting, 6.6f);

        // when
        setting.Save(null!);

        // then
        Assert.Equal(7, _entry.Value);
    }

    [Fact]
    public void A_config_edit_moves_the_slider_value()
    {
        // given
        PinLimitSetting setting = LoadedSetting();

        // when
        _entry.Value = 10;

        // then
        Assert.Equal(10f, setting.Value);
    }
}

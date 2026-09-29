using BepInEx.Configuration;
using BepInEx.Logging;

namespace PeakAchiever.Pinning;

/// <summary>Keeps the pin board in the BepInEx config file, so pins survive from one run to the next.</summary>
internal sealed class PinnedBadgesStore
{
    private const string Section = "Tracker";
    private const int DefaultCapacity = 5;
    private const int MaxCapacity = 12;
    private const int DefaultTeamCapacity = 5;

    private readonly ConfigEntry<string> _pins;
    private readonly ConfigEntry<int> _capacity;
    private readonly ConfigEntry<int> _teamCapacity;
    private readonly ManualLogSource _log;

    public PinnedBadgesStore(ConfigFile config, ManualLogSource log)
    {
        _log = log;
        _pins = config.Bind(
            Section,
            "PinnedBadges",
            "",
            "Badges pinned to the tracker, in order; a leading + marks one pinned while already earned, to help an ally. "
                + "Edited from the pause menu badges page."
        );
        _capacity = config.Bind(
            Section,
            "MaxPinnedBadges",
            DefaultCapacity,
            new ConfigDescription(
                "How many badges can be pinned at once.",
                new AcceptableValueRange<int>(1, MaxCapacity)
            )
        );
        _teamCapacity = config.Bind(
            Section,
            "MaxTeamPins",
            DefaultTeamCapacity,
            new ConfigDescription(
                "How many badges the host can pin for the whole team in a multiplayer game, on top of their own.",
                new AcceptableValueRange<int>(1, MaxCapacity)
            )
        );
        PinList stored = PinList.Read(_pins.Value);
        foreach (string name in stored.Unknown)
            _log.LogWarning($"Ignoring unknown pinned badge '{name}' in the config.");
        Board = new PinBoard(stored.Pins, _capacity.Value, stored.ForAllies);
        _capacity.SettingChanged += (_, _) => Board.Capacity = _capacity.Value;
    }

    public PinBoard Board { get; }

    /// <summary>How many badges the host can pin for the team (the MaxTeamPins setting).</summary>
    public int TeamCapacity => _teamCapacity.Value;

    public void Save() => _pins.Value = PinList.Write(Board);
}

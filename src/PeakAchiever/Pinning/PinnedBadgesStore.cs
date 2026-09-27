using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace PeakAchiever.Pinning;

/// <summary>Keeps the pin board in the BepInEx config file, so pins survive from one run to the next.</summary>
internal sealed class PinnedBadgesStore
{
    private const string Section = "Tracker";
    private const char Separator = ',';
    private const int DefaultCapacity = 5;
    private const int MaxCapacity = 12;

    private readonly ConfigEntry<string> _pins;
    private readonly ConfigEntry<int> _capacity;
    private readonly ManualLogSource _log;

    public PinnedBadgesStore(ConfigFile config, ManualLogSource log)
    {
        _log = log;
        _pins = config.Bind(
            Section,
            "PinnedBadges",
            "",
            "Badges pinned to the tracker, in order. Edited from the pause menu badges page."
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
        Board = new PinBoard(ParsePins(_pins.Value).Distinct(), _capacity.Value);
        _capacity.SettingChanged += (_, _) => Board.Capacity = _capacity.Value;
    }

    public PinBoard Board { get; }

    public void Save() => _pins.Value = string.Join(Separator.ToString(), Board.Pins);

    private IEnumerable<ACHIEVEMENTTYPE> ParsePins(string stored)
    {
        foreach (string name in stored.Split([Separator], StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (Enum.TryParse(name, out ACHIEVEMENTTYPE badge) && badge != ACHIEVEMENTTYPE.NONE)
                yield return badge;
            else
                _log.LogWarning($"Ignoring unknown pinned badge '{name}' in the config.");
        }
    }
}

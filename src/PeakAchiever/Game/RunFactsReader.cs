using System;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>Translates the game's run state into a <see cref="RunFacts"/> snapshot.</summary>
internal static class RunFactsReader
{
    /// <summary>
    /// True while a run is in progress and every source the snapshot reads exists.
    /// The airport has a character but no map, and <see cref="AchievementManager.Initialized"/>
    /// only turns true once the game allocated the run-based values.
    /// </summary>
    public static bool IsInRun =>
        Character.localCharacter != null
        && !Character.localCharacter.inAirport
        && MapHandler.ExistsAndInitialized
        && RunManager.Instance != null
        && AchievementManager.Initialized;

    public static RunFacts Read()
    {
        AchievementManager achievements = Singleton<AchievementManager>.Instance;
        MapHandler map = Singleton<MapHandler>.Instance;
        SerializableRunBasedValues run = achievements.runBasedValueData;
        return new RunFacts(
            ReadRunValues(run),
            ReadCollectionCounts(run),
            ReadLifetimeStats(achievements),
            map.segments.Select(segment => segment.biome).ToArray(),
            map.biomes.ToArray(),
            (int)map.GetCurrentSegment(),
            RunManager.Instance.TimeSinceRunStarted,
            Character.AllCharacters.Count
        );
    }

    // Read the dictionaries directly: AchievementManager.GetRunBasedInt/Float write a zero
    // for a missing key, which raises Player.OnAchievementProgressChanged and so a refresh loop.
    private static Dictionary<RUNBASEDVALUETYPE, float> ReadRunValues(SerializableRunBasedValues run)
    {
        var values = new Dictionary<RUNBASEDVALUETYPE, float>();
        foreach (KeyValuePair<RUNBASEDVALUETYPE, int> entry in run.runBasedInts)
            values[entry.Key] = entry.Value;
        // The game checks run-based badges against both tables (AchievementManager.RunBasedAchievementData.IsAchieved).
        foreach (KeyValuePair<RUNBASEDVALUETYPE, float> entry in run.runBasedFloats)
            values[entry.Key] = values.TryGetValue(entry.Key, out float fromInt) ? Math.Max(fromInt, entry.Value) : entry.Value;
        return values;
    }

    private static Dictionary<RunCollection, int> ReadCollectionCounts(SerializableRunBasedValues run) =>
        new()
        {
            [RunCollection.DifferentBerriesEaten] = run.runBasedFruitsEaten.Count,
            [RunCollection.DifferentShroomBerriesEaten] = run.shroomBerriesEaten.Count,
            [RunCollection.DifferentNonToxicMushroomsEaten] = run.nonToxicMushroomsEaten.Count,
            [RunCollection.GourmandDishesEaten] = run.gourmandRequirementsEaten.Count,
        };

    private static Dictionary<STEAMSTATTYPE, int> ReadLifetimeStats(AchievementManager achievements)
    {
        var stats = new Dictionary<STEAMSTATTYPE, int>();
        foreach (STEAMSTATTYPE stat in BadgeRules.LifetimeStats)
        {
            if (achievements.GetSteamStatInt(stat, out int value))
                stats[stat] = value;
        }
        return stats;
    }
}

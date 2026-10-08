using System;
using System.Collections.Generic;
using System.Linq;
using Peak;
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

    /// <summary>
    /// True when the local scout joined after the first segment: their timeline only starts then
    /// (MountainProgressHandler.JoinedInSegment stays -1 for a scout there from the start, v2.4.c).
    /// </summary>
    private static bool JoinedMidRun => Singleton<MountainProgressHandler>.Instance.JoinedInSegment >= 0;

    // The Nadir's segment is appended to the array once the map has a VoidBiome (MapHandler.SetUpVoidSegment
    // adds VoidBiome.instance.segment, v2.6.b). No step of the climb, it is left out of the order. == null,
    // not ?.: Unity objects override the null check.
    private static bool IsNadir(MapHandler.MapSegment segment) => VoidBiome.instance != null && segment == VoidBiome.instance.segment;

    public static RunFacts Read(SplitHistory history)
    {
        AchievementManager achievements = Singleton<AchievementManager>.Instance;
        MapHandler map = Singleton<MapHandler>.Instance;
        SerializableRunBasedValues run = achievements.runBasedValueData;
        float secondsSinceRunStarted = RunManager.Instance.TimeSinceRunStarted;
        return new RunFacts(
            ReadRunValues(run),
            ReadEatenItems(run),
            ItemCatalog.CollectionCandidates,
            ReadLifetimeStats(achievements),
            map.segments.Where(segment => !IsNadir(segment)).Select(segment => segment.biome).ToArray(),
            map.biomes.ToArray(),
            // Outside the Nadir, GetCurrentSegment is the segment's index (MapHandler.currentSegment); in it,
            // Segment.Void, which is past the array (v2.6.b).
            map.GetCurrentSegment() == Segment.Void ? null : (int)map.GetCurrentSegment(),
            secondsSinceRunStarted,
            BiomeTimeline.Split(ReadTimeline(), secondsSinceRunStarted, JoinedMidRun),
            history.MediansAt(Ascents.currentAscent),
            MapItemScanner.ItemsOnMap,
            RunOutcome.Lost,
            // The bells register themselves while their segment is loaded (GloomSafeZone.OnEnable, v2.6.b).
            GloomSafeZone.ALL_GLOOM_SAFE_ZONES.OfType<GhostFire>().Count(bell => bell.isLit)
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

    // CharacterStats.Record samples the local scout's biome (the current segment's) and the run time, in
    // whole seconds, about once a second for the end screen; ReconnectData restores it on a rejoin (v2.4.c).
    private static (Biome.BiomeType, float)[] ReadTimeline() =>
        Character.localCharacter.refs.stats.timelineInfo.Select(sample => (sample.biome, sample.time)).ToArray();

    // Copied: the game keeps adding to these lists while the snapshot is read.
    private static Dictionary<RunCollection, IReadOnlyCollection<ushort>> ReadEatenItems(SerializableRunBasedValues run) =>
        new()
        {
            [RunCollection.DifferentBerriesEaten] = run.runBasedFruitsEaten.ToHashSet(),
            [RunCollection.DifferentShroomBerriesEaten] = run.shroomBerriesEaten.ToHashSet(),
            [RunCollection.DifferentNonToxicMushroomsEaten] = run.nonToxicMushroomsEaten.ToHashSet(),
            [RunCollection.GourmandDishesEaten] = run.gourmandRequirementsEaten.ToHashSet(),
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

using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>
/// Immutable snapshot of everything the badge rules need about the current run,
/// read from the game once per refresh so rules stay pure.
/// </summary>
internal sealed class RunFacts
{
    public RunFacts(
        IReadOnlyDictionary<RUNBASEDVALUETYPE, float> runValues,
        IReadOnlyDictionary<RunCollection, IReadOnlyCollection<ushort>> eatenItems,
        IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>> collectionCandidates,
        IReadOnlyDictionary<STEAMSTATTYPE, int> lifetimeStats,
        IReadOnlyList<Biome.BiomeType> segmentBiomes,
        IReadOnlyCollection<Biome.BiomeType> presentBiomes,
        int currentSegmentIndex,
        float secondsSinceRunStarted,
        IReadOnlyList<BiomeSplit> biomeSplits,
        int scoutCount
    )
    {
        RunValues = runValues;
        EatenItems = eatenItems;
        CollectionCandidates = collectionCandidates;
        LifetimeStats = lifetimeStats;
        SegmentBiomes = segmentBiomes;
        PresentBiomes = presentBiomes;
        CurrentSegmentIndex = currentSegmentIndex;
        SecondsSinceRunStarted = secondsSinceRunStarted;
        BiomeSplits = biomeSplits;
        ScoutCount = scoutCount;
    }

    /// <summary>Run counters, the higher of the game's int and float tables for each key.</summary>
    public IReadOnlyDictionary<RUNBASEDVALUETYPE, float> RunValues { get; }

    /// <summary>The distinct item ids of each list eaten this run.</summary>
    public IReadOnlyDictionary<RunCollection, IReadOnlyCollection<ushort>> EatenItems { get; }

    /// <summary>Every item id of the game that eating would add to each list.</summary>
    public IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>> CollectionCandidates { get; }

    public IReadOnlyDictionary<STEAMSTATTYPE, int> LifetimeStats { get; }

    /// <summary>Biome of each map segment, in climbing order.</summary>
    public IReadOnlyList<Biome.BiomeType> SegmentBiomes { get; }

    public IReadOnlyCollection<Biome.BiomeType> PresentBiomes { get; }

    public int CurrentSegmentIndex { get; }

    public float SecondsSinceRunStarted { get; }

    /// <summary>Time spent in each biome so far, in climbing order.</summary>
    public IReadOnlyList<BiomeSplit> BiomeSplits { get; }

    public int ScoutCount { get; }

    public float RunValue(RUNBASEDVALUETYPE type) =>
        RunValues.TryGetValue(type, out float value) ? value : 0f;

    public int CollectionCount(RunCollection collection) =>
        EatenItems.TryGetValue(collection, out IReadOnlyCollection<ushort> eaten) ? eaten.Count : 0;

    public int LifetimeStat(STEAMSTATTYPE stat) =>
        LifetimeStats.TryGetValue(stat, out int value) ? value : 0;

    /// <summary>True once every segment of this biome lies behind the current segment.</summary>
    public bool HasLeft(Biome.BiomeType biome)
    {
        bool seenBehind = false;
        for (int segment = 0; segment < SegmentBiomes.Count; segment++)
        {
            if (SegmentBiomes[segment] != biome)
                continue;
            if (segment >= CurrentSegmentIndex)
                return false;
            seenBehind = true;
        }
        return seenBehind;
    }
}

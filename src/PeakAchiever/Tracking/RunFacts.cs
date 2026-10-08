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
        int? currentSegmentIndex,
        float secondsSinceRunStarted,
        IReadOnlyList<BiomeSplit> biomeSplits,
        IReadOnlyDictionary<Biome.BiomeType, float> biomeMedians,
        IReadOnlyCollection<ushort>? itemsOnMap,
        bool runLost,
        int litBells = 0
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
        BiomeMedians = biomeMedians;
        ItemsOnMap = itemsOnMap;
        RunLost = runLost;
        LitBells = litBells;
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

    /// <summary>
    /// The segment the team is in, as an index of <see cref="SegmentBiomes"/>; null in the Nadir, which is
    /// none of the map's segments.
    /// </summary>
    public int? CurrentSegmentIndex { get; }

    public float SecondsSinceRunStarted { get; }

    /// <summary>Time spent in each biome so far, in climbing order.</summary>
    public IReadOnlyList<BiomeSplit> BiomeSplits { get; }

    /// <summary>The median time past runs at this run's ascent took to finish each biome.</summary>
    public IReadOnlyDictionary<Biome.BiomeType, float> BiomeMedians { get; }

    /// <summary>Every item id this map can yield, or null when the map could not be read.</summary>
    public IReadOnlyCollection<ushort>? ItemsOnMap { get; }

    /// <summary>True once the run has ended with nobody at the summit.</summary>
    public bool RunLost { get; }

    /// <summary>The Gloom's belltowers lit so far, by anyone in the team.</summary>
    public int LitBells { get; }

    public float RunValue(RUNBASEDVALUETYPE type) =>
        RunValues.TryGetValue(type, out float value) ? value : 0f;

    public int CollectionCount(RunCollection collection) =>
        EatenItems.TryGetValue(collection, out IReadOnlyCollection<ushort> eaten) ? eaten.Count : 0;

    public int LifetimeStat(STEAMSTATTYPE stat) =>
        LifetimeStats.TryGetValue(stat, out int value) ? value : 0;

    /// <summary>
    /// True once every segment of this biome lies behind the current segment. Never in the Nadir: an item
    /// takes the team there from any segment, and Action_WarpToBiome can send it to any segment (v2.6.b).
    /// </summary>
    public bool HasLeft(Biome.BiomeType biome)
    {
        if (CurrentSegmentIndex is not { } current)
            return false;
        bool seenBehind = false;
        for (int segment = 0; segment < SegmentBiomes.Count; segment++)
        {
            if (SegmentBiomes[segment] != biome)
                continue;
            if (segment >= current)
                return false;
            seenBehind = true;
        }
        return seenBehind;
    }
}

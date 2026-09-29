using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

/// <summary>Builds a <see cref="RunFacts"/> for a test, defaulting to a fresh solo run on the beach.</summary>
internal sealed class RunFactsBuilder
{
    private readonly Dictionary<RUNBASEDVALUETYPE, float> _runValues = [];
    private readonly Dictionary<RunCollection, IReadOnlyCollection<ushort>> _eaten = [];
    private readonly Dictionary<RunCollection, IReadOnlyList<ushort>> _candidates = [];
    private readonly Dictionary<STEAMSTATTYPE, int> _lifetimeStats = [];
    private Biome.BiomeType[] _segmentBiomes =
    [
        Biome.BiomeType.Shore,
        Biome.BiomeType.Tropics,
        Biome.BiomeType.Alpine,
        Biome.BiomeType.Volcano,
        Biome.BiomeType.Volcano,
        Biome.BiomeType.Peak,
    ];
    private int _currentSegment;
    private float _seconds;
    private BiomeSplit[] _biomeSplits = [];
    private IReadOnlyDictionary<Biome.BiomeType, float> _biomeMedians = new Dictionary<Biome.BiomeType, float>();
    private IReadOnlyCollection<ushort>? _itemsOnMap;

    public RunFactsBuilder WithRunValue(RUNBASEDVALUETYPE type, float value)
    {
        _runValues[type] = value;
        return this;
    }

    /// <summary>That many distinct items eaten, whichever they are.</summary>
    public RunFactsBuilder WithCollection(RunCollection collection, int count) =>
        WithEaten(collection, Enumerable.Range(0, count).Select(id => (ushort)id).ToArray());

    public RunFactsBuilder WithEaten(RunCollection collection, params ushort[] items)
    {
        _eaten[collection] = items;
        return this;
    }

    public RunFactsBuilder WithCandidates(RunCollection collection, params ushort[] items)
    {
        _candidates[collection] = items;
        return this;
    }

    public RunFactsBuilder WithLifetimeStat(STEAMSTATTYPE stat, int value)
    {
        _lifetimeStats[stat] = value;
        return this;
    }

    public RunFactsBuilder WithSegments(params Biome.BiomeType[] biomes)
    {
        _segmentBiomes = biomes;
        return this;
    }

    public RunFactsBuilder AtSegment(int segment)
    {
        _currentSegment = segment;
        return this;
    }

    public RunFactsBuilder AfterSeconds(float seconds)
    {
        _seconds = seconds;
        return this;
    }

    public RunFactsBuilder WithBiomeSplits(params BiomeSplit[] splits)
    {
        _biomeSplits = splits;
        return this;
    }

    public RunFactsBuilder WithBiomeMedians(IReadOnlyDictionary<Biome.BiomeType, float> medians)
    {
        _biomeMedians = medians;
        return this;
    }

    public RunFactsBuilder WithItemsOnMap(params ushort[] items)
    {
        _itemsOnMap = items;
        return this;
    }

    public RunFacts Build() =>
        new(
            _runValues,
            _eaten,
            _candidates,
            _lifetimeStats,
            _segmentBiomes,
            _segmentBiomes.Distinct().ToArray(),
            _currentSegment,
            _seconds,
            _biomeSplits,
            _biomeMedians,
            _itemsOnMap
        );
}

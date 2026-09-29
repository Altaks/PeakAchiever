using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

/// <summary>Builds a <see cref="RunFacts"/> for a test, defaulting to a fresh solo run on the beach.</summary>
internal sealed class RunFactsBuilder
{
    private readonly Dictionary<RUNBASEDVALUETYPE, float> _runValues = [];
    private readonly Dictionary<RunCollection, int> _collections = [];
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

    public RunFactsBuilder WithRunValue(RUNBASEDVALUETYPE type, float value)
    {
        _runValues[type] = value;
        return this;
    }

    public RunFactsBuilder WithCollection(RunCollection collection, int count)
    {
        _collections[collection] = count;
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

    public RunFacts Build() =>
        new(
            _runValues,
            _collections,
            _lifetimeStats,
            _segmentBiomes,
            _segmentBiomes.Distinct().ToArray(),
            _currentSegment,
            _seconds
        );
}

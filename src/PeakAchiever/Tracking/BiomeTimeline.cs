using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>
/// Time spent in one stretch of a biome; the current one is still counting. Not whole when the scout
/// joined mid-run and saw only its end.
/// </summary>
internal readonly record struct BiomeSplit(Biome.BiomeType Biome, float Seconds, bool IsCurrent, bool IsWhole = true);

/// <summary>A biome's time against the median of past runs at this ascent; null when there is nothing to show.</summary>
internal readonly record struct ComparedSplit(BiomeSplit Split, float? DeltaSeconds)
{
    /// <summary>
    /// A finished biome shows how far it is from the median either way; the current one only once it runs
    /// over, since being under the median says nothing until the biome ends.
    /// </summary>
    public static ComparedSplit Against(BiomeSplit split, IReadOnlyDictionary<Biome.BiomeType, float> medians)
    {
        if (!split.IsWhole || !medians.TryGetValue(split.Biome, out float median))
            return new ComparedSplit(split, null);
        float delta = split.Seconds - median;
        return new ComparedSplit(split, split.IsCurrent && delta <= 0f ? null : delta);
    }
}

internal static class BiomeTimeline
{
    /// <summary>
    /// Cuts the run into consecutive biome stretches. The first starts at the run's start (or on arrival,
    /// for a scout who joined mid-run), each later one at its first sample, and the last runs to now.
    /// </summary>
    public static IReadOnlyList<BiomeSplit> Split(
        IReadOnlyList<(Biome.BiomeType Biome, float Seconds)> samples,
        float secondsSinceRunStarted,
        bool joinedMidRun
    )
    {
        var splits = new List<BiomeSplit>();
        if (samples.Count == 0)
            return splits;
        Biome.BiomeType biome = samples[0].Biome;
        float start = joinedMidRun ? samples[0].Seconds : 0f;
        foreach ((Biome.BiomeType sampleBiome, float seconds) in samples)
        {
            if (sampleBiome == biome)
                continue;
            splits.Add(new BiomeSplit(biome, seconds - start, IsCurrent: false, IsWhole: splits.Count > 0 || !joinedMidRun));
            biome = sampleBiome;
            start = seconds;
        }
        splits.Add(new BiomeSplit(biome, secondsSinceRunStarted - start, IsCurrent: true, IsWhole: splits.Count > 0 || !joinedMidRun));
        return splits;
    }
}

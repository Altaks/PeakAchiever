using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>Time spent in one stretch of a biome; the current one is still counting.</summary>
internal readonly record struct BiomeSplit(Biome.BiomeType Biome, float Seconds, bool IsCurrent);

internal static class BiomeTimeline
{
    /// <summary>
    /// Cuts the run into consecutive biome stretches. The first starts at the run's start, each later one
    /// at its first sample, and the last runs to now.
    /// </summary>
    public static IReadOnlyList<BiomeSplit> Split(
        IReadOnlyList<(Biome.BiomeType Biome, float Seconds)> samples,
        float secondsSinceRunStarted
    )
    {
        var splits = new List<BiomeSplit>();
        if (samples.Count == 0)
            return splits;
        Biome.BiomeType biome = samples[0].Biome;
        float start = 0f;
        foreach ((Biome.BiomeType sampleBiome, float seconds) in samples)
        {
            if (sampleBiome == biome)
                continue;
            splits.Add(new BiomeSplit(biome, seconds - start, IsCurrent: false));
            biome = sampleBiome;
            start = seconds;
        }
        splits.Add(new BiomeSplit(biome, secondsSinceRunStarted - start, IsCurrent: true));
        return splits;
    }
}

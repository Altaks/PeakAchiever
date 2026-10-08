using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>When the run should reach the summit, going by past runs at the same ascent.</summary>
internal static class RunEta
{
    /// <returns>
    /// The run time the summit should be reached at, or null until the timeline has started and every
    /// biome left to climb has been finished in a past run. Null in the Nadir, off the map's segments.
    /// </returns>
    public static float? Estimate(RunFacts facts)
    {
        if (facts.BiomeSplits.Count == 0 || facts.CurrentSegmentIndex is not { } segment)
            return null;
        BiomeSplit current = facts.BiomeSplits[facts.BiomeSplits.Count - 1];
        if (!facts.BiomeMedians.TryGetValue(current.Biome, out float currentMedian))
            return null;
        float eta = facts.SecondsSinceRunStarted - current.Seconds + System.Math.Max(currentMedian, current.Seconds);
        foreach (Biome.BiomeType biome in BiomesAhead(facts.SegmentBiomes, segment))
        {
            if (!facts.BiomeMedians.TryGetValue(biome, out float median))
                return null;
            eta += median;
        }
        return eta;
    }

    // Consecutive segments of one biome make a single stretch on the timeline (BiomeTimeline.Split).
    private static IEnumerable<Biome.BiomeType> BiomesAhead(IReadOnlyList<Biome.BiomeType> segmentBiomes, int segment)
    {
        Biome.BiomeType previous = segmentBiomes[segment];
        foreach (Biome.BiomeType biome in segmentBiomes.Skip(segment + 1))
        {
            if (biome != previous)
                yield return biome;
            previous = biome;
        }
    }
}

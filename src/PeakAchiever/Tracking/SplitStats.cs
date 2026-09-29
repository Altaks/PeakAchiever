using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>How a biome went across past runs at one ascent.</summary>
internal readonly record struct BiomeStats(Biome.BiomeType Biome, int Times, float Median, float Best);

/// <summary>A whole climb of one map layout, from the medians and from the best times.</summary>
internal readonly record struct LayoutTotal(float Median, float Best);

internal static class SplitStats
{
    /// <summary>
    /// The climb of <paramref name="layout"/>: its biomes, plus those the level table lists on no layout
    /// (the Peak, on every map). Null while one of the layout's biomes has no time yet.
    /// </summary>
    public static LayoutTotal? TotalFor(
        IReadOnlyCollection<Biome.BiomeType> layout,
        IEnumerable<IReadOnlyCollection<Biome.BiomeType>> allLayouts,
        IReadOnlyList<BiomeStats> stats
    )
    {
        if (!layout.All(biome => stats.Any(row => row.Biome == biome)))
            return null;
        HashSet<Biome.BiomeType> listed = allLayouts.SelectMany(map => map).ToHashSet();
        BiomeStats[] climbed = stats.Where(row => layout.Contains(row.Biome) || !listed.Contains(row.Biome)).ToArray();
        return new LayoutTotal(climbed.Sum(row => row.Median), climbed.Sum(row => row.Best));
    }
}

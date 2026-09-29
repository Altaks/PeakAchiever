using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class SplitStatsTests
{
    private static readonly Guid FirstRun = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondRun = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ThirdRun = new("33333333-3333-3333-3333-333333333333");

    private static readonly Biome.BiomeType[] TropicsLayout =
    [
        Biome.BiomeType.Shore,
        Biome.BiomeType.Tropics,
        Biome.BiomeType.Alpine,
        Biome.BiomeType.Volcano,
    ];

    private static readonly Biome.BiomeType[] RootsLayout =
    [
        Biome.BiomeType.Shore,
        Biome.BiomeType.Roots,
        Biome.BiomeType.Mesa,
        Biome.BiomeType.Swamp,
    ];

    [Fact]
    public void Each_biome_counts_its_times_median_and_best_in_climbing_order()
    {
        // given
        var history = new SplitHistory(
            [
                new PastSplit(FirstRun, 2, 1, Biome.BiomeType.Tropics, 700f),
                new PastSplit(FirstRun, 2, 0, Biome.BiomeType.Shore, 500f),
                new PastSplit(SecondRun, 2, 0, Biome.BiomeType.Shore, 300f),
                new PastSplit(ThirdRun, 2, 0, Biome.BiomeType.Shore, 400f),
                new PastSplit(ThirdRun, 5, 0, Biome.BiomeType.Shore, 100f),
            ]
        );

        // when
        IReadOnlyList<BiomeStats> stats = history.StatsAt(2);

        // then
        Assert.Equal(
            [new BiomeStats(Biome.BiomeType.Shore, Times: 3, Median: 400f, Best: 300f), new BiomeStats(Biome.BiomeType.Tropics, 1, 700f, 700f)],
            stats
        );
    }

    [Fact]
    public void The_ascents_with_a_history_are_listed_in_order()
    {
        // given
        var history = new SplitHistory(
            [
                new PastSplit(FirstRun, 5, 0, Biome.BiomeType.Shore, 1f),
                new PastSplit(SecondRun, -1, 0, Biome.BiomeType.Shore, 1f),
                new PastSplit(ThirdRun, 5, 0, Biome.BiomeType.Shore, 1f),
            ]
        );

        // when
        IReadOnlyList<int> ascents = history.Ascents;

        // then
        Assert.Equal([-1, 5], ascents);
    }

    [Fact]
    public void Erasing_an_ascent_keeps_the_others()
    {
        // given
        var history = new SplitHistory(
            [new PastSplit(FirstRun, 2, 0, Biome.BiomeType.Shore, 500f), new PastSplit(SecondRun, 3, 0, Biome.BiomeType.Shore, 300f)]
        );

        // when
        int erased = history.RemoveAscent(2);

        // then
        Assert.Equal(1, erased);
        Assert.Equal([new PastSplit(SecondRun, 3, 0, Biome.BiomeType.Shore, 300f)], history.Splits);
    }

    [Fact]
    public void A_layout_total_adds_its_biomes_and_those_on_every_map()
    {
        // given: Peak is on every map but in no layout of the level table
        BiomeStats[] stats =
        [
            new(Biome.BiomeType.Shore, 3, Median: 400f, Best: 300f),
            new(Biome.BiomeType.Tropics, 2, 600f, 500f),
            new(Biome.BiomeType.Alpine, 2, 700f, 650f),
            new(Biome.BiomeType.Volcano, 1, 800f, 800f),
            new(Biome.BiomeType.Peak, 1, 100f, 90f),
            new(Biome.BiomeType.Roots, 1, 900f, 900f),
        ];

        // when
        LayoutTotal? total = SplitStats.TotalFor(TropicsLayout, [TropicsLayout, RootsLayout], stats);

        // then
        Assert.Equal(new LayoutTotal(Median: 2600f, Best: 2340f), total);
    }

    [Fact]
    public void No_layout_total_while_one_of_its_biomes_has_no_time()
    {
        // given
        BiomeStats[] stats = [new(Biome.BiomeType.Shore, 3, 400f, 300f), new(Biome.BiomeType.Roots, 1, 900f, 900f)];

        // when
        LayoutTotal? total = SplitStats.TotalFor(RootsLayout, [TropicsLayout, RootsLayout], stats);

        // then
        Assert.Null(total);
    }
}

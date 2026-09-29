using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BiomeTimelineTests
{
    [Fact]
    public void No_sample_yet_gives_no_split()
    {
        // given
        (Biome.BiomeType, float)[] samples = [];

        // when
        IReadOnlyList<BiomeSplit> splits = BiomeTimeline.Split(samples, secondsSinceRunStarted: 12f);

        // then
        Assert.Empty(splits);
    }

    [Fact]
    public void Each_biome_lasts_until_the_next_one_starts_and_the_last_one_runs_to_now()
    {
        // given
        (Biome.BiomeType, float)[] samples =
        [
            (Biome.BiomeType.Shore, 3f),
            (Biome.BiomeType.Shore, 4f),
            (Biome.BiomeType.Tropics, 500f),
            (Biome.BiomeType.Tropics, 501f),
        ];

        // when
        IReadOnlyList<BiomeSplit> splits = BiomeTimeline.Split(samples, secondsSinceRunStarted: 800f);

        // then
        Assert.Equal(
            [new BiomeSplit(Biome.BiomeType.Shore, 500f, IsCurrent: false), new BiomeSplit(Biome.BiomeType.Tropics, 300f, IsCurrent: true)],
            splits
        );
    }

    [Fact]
    public void A_biome_entered_again_later_gets_its_own_split()
    {
        // given
        (Biome.BiomeType, float)[] samples =
        [
            (Biome.BiomeType.Peak, 10f),
            (Biome.BiomeType.Void, 20f),
            (Biome.BiomeType.Peak, 50f),
        ];

        // when
        IReadOnlyList<BiomeSplit> splits = BiomeTimeline.Split(samples, secondsSinceRunStarted: 60f);

        // then
        Assert.Equal(
            [
                new BiomeSplit(Biome.BiomeType.Peak, 20f, IsCurrent: false),
                new BiomeSplit(Biome.BiomeType.Void, 30f, IsCurrent: false),
                new BiomeSplit(Biome.BiomeType.Peak, 10f, IsCurrent: true),
            ],
            splits
        );
    }
}

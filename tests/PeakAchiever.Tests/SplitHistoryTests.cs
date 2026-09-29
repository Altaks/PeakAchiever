using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class SplitHistoryTests
{
    private static readonly Guid FirstRun = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondRun = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ThirdRun = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void A_split_reads_back_from_its_line()
    {
        // given
        var split = new PastSplit(FirstRun, Ascent: 3, Index: 1, Biome.BiomeType.Tropics, 612.5f);

        // when
        bool parsed = PastSplit.TryParse(split.ToLine(), out PastSplit read);

        // then
        Assert.True(parsed);
        Assert.Equal(split, read);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not,a,split")]
    [InlineData("11111111-1111-1111-1111-111111111111,3,1,NoSuchBiome,612.5")]
    [InlineData("11111111-1111-1111-1111-111111111111,3,1,99,612.5")]
    [InlineData("11111111-1111-1111-1111-111111111111,3,1,Tropics,soon")]
    public void A_malformed_line_is_rejected(string line)
    {
        // when
        bool parsed = PastSplit.TryParse(line, out _);

        // then
        Assert.False(parsed);
    }

    [Fact]
    public void The_same_split_of_the_same_run_is_kept_once()
    {
        // given
        var history = new SplitHistory([new PastSplit(FirstRun, 0, 0, Biome.BiomeType.Shore, 400f)]);

        // when
        bool added = history.Add(new PastSplit(FirstRun, 0, 0, Biome.BiomeType.Shore, 401f));

        // then
        Assert.False(added);
        Assert.Equal(400f, history.MediansAt(0)[Biome.BiomeType.Shore]);
    }

    [Fact]
    public void The_median_takes_the_middle_split_or_the_mean_of_the_two_middle_ones()
    {
        // given
        var history = new SplitHistory(
            [
                new PastSplit(FirstRun, 0, 0, Biome.BiomeType.Shore, 300f),
                new PastSplit(SecondRun, 0, 0, Biome.BiomeType.Shore, 900f),
                new PastSplit(ThirdRun, 0, 0, Biome.BiomeType.Shore, 400f),
                new PastSplit(FirstRun, 0, 1, Biome.BiomeType.Tropics, 500f),
                new PastSplit(SecondRun, 0, 1, Biome.BiomeType.Tropics, 700f),
            ]
        );

        // when
        IReadOnlyDictionary<Biome.BiomeType, float> medians = history.MediansAt(0);

        // then
        Assert.Equal(400f, medians[Biome.BiomeType.Shore]);
        Assert.Equal(600f, medians[Biome.BiomeType.Tropics]);
    }

    [Fact]
    public void Each_ascent_keeps_its_own_medians()
    {
        // given
        var history = new SplitHistory(
            [
                new PastSplit(FirstRun, 0, 0, Biome.BiomeType.Shore, 300f),
                new PastSplit(SecondRun, 4, 0, Biome.BiomeType.Shore, 800f),
            ]
        );

        // when
        IReadOnlyDictionary<Biome.BiomeType, float> medians = history.MediansAt(4);

        // then
        Assert.Equal(new Dictionary<Biome.BiomeType, float> { [Biome.BiomeType.Shore] = 800f }, medians);
    }
}

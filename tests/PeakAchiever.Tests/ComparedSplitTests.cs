using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class ComparedSplitTests
{
    private static readonly Dictionary<Biome.BiomeType, float> Medians = new()
    {
        [Biome.BiomeType.Shore] = 500f,
        [Biome.BiomeType.Tropics] = 800f,
    };

    [Theory]
    [InlineData(420f, -80f)]
    [InlineData(580f, 80f)]
    public void A_finished_biome_compares_with_its_median(float seconds, float delta)
    {
        // given
        var split = new BiomeSplit(Biome.BiomeType.Shore, seconds, IsCurrent: false);

        // when
        ComparedSplit compared = ComparedSplit.Against(split, Medians);

        // then
        Assert.Equal(delta, compared.DeltaSeconds);
    }

    [Theory]
    [InlineData(300f, null)]
    [InlineData(900f, 100f)]
    public void The_current_biome_only_shows_the_time_it_runs_over(float seconds, float? delta)
    {
        // given
        var split = new BiomeSplit(Biome.BiomeType.Tropics, seconds, IsCurrent: true);

        // when
        ComparedSplit compared = ComparedSplit.Against(split, Medians);

        // then
        Assert.Equal(delta, compared.DeltaSeconds);
    }

    [Fact]
    public void A_biome_with_no_past_time_has_nothing_to_compare_with()
    {
        // given
        var split = new BiomeSplit(Biome.BiomeType.Alpine, 700f, IsCurrent: false);

        // when
        ComparedSplit compared = ComparedSplit.Against(split, Medians);

        // then
        Assert.Null(compared.DeltaSeconds);
    }

    [Fact]
    public void A_biome_seen_only_in_part_is_not_compared()
    {
        // given
        var split = new BiomeSplit(Biome.BiomeType.Shore, 120f, IsCurrent: false, IsWhole: false);

        // when
        ComparedSplit compared = ComparedSplit.Against(split, Medians);

        // then
        Assert.Null(compared.DeltaSeconds);
    }
}

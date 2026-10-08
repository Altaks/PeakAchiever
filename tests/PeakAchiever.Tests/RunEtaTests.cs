using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class RunEtaTests
{
    // Medians for the builder's default map: Shore, Tropics, Alpine, Volcano (two segments), Peak.
    private static readonly Dictionary<Biome.BiomeType, float> EveryBiome = new()
    {
        [Biome.BiomeType.Shore] = 500f,
        [Biome.BiomeType.Tropics] = 600f,
        [Biome.BiomeType.Alpine] = 700f,
        [Biome.BiomeType.Volcano] = 800f,
        [Biome.BiomeType.Peak] = 100f,
    };

    [Fact]
    public void The_eta_adds_the_median_of_the_rest_of_this_biome_and_of_every_biome_ahead()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .AtSegment(1)
            .AfterSeconds(650f)
            .WithBiomeSplits(new(Biome.BiomeType.Shore, 450f, IsCurrent: false), new(Biome.BiomeType.Tropics, 200f, IsCurrent: true))
            .WithBiomeMedians(EveryBiome)
            .Build();

        // when
        float? eta = RunEta.Estimate(facts);

        // then: 450 done, 600 for the Tropics, then 700 + 800 + 100 ahead, the two Volcano segments once
        Assert.Equal(2650f, eta);
    }

    [Fact]
    public void A_biome_already_longer_than_its_median_counts_the_time_spent_so_far()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .AtSegment(1)
            .AfterSeconds(1250f)
            .WithBiomeSplits(new(Biome.BiomeType.Shore, 450f, IsCurrent: false), new(Biome.BiomeType.Tropics, 800f, IsCurrent: true))
            .WithBiomeMedians(EveryBiome)
            .Build();

        // when
        float? eta = RunEta.Estimate(facts);

        // then
        Assert.Equal(1250f + 700f + 800f + 100f, eta);
    }

    [Fact]
    public void No_eta_in_the_nadir()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .InNadir()
            .AfterSeconds(2000f)
            .WithBiomeSplits(new(Biome.BiomeType.Shore, 450f, IsCurrent: false), new(Biome.BiomeType.Void, 30f, IsCurrent: true))
            .WithBiomeMedians(new Dictionary<Biome.BiomeType, float>(EveryBiome) { [Biome.BiomeType.Void] = 60f })
            .Build();

        // when
        float? eta = RunEta.Estimate(facts);

        // then: the Nadir is none of the map's segments, so nothing ahead can be counted
        Assert.Null(eta);
    }

    [Fact]
    public void No_eta_while_a_biome_ahead_has_no_past_split()
    {
        // given
        var medians = new Dictionary<Biome.BiomeType, float>(EveryBiome);
        medians.Remove(Biome.BiomeType.Peak);
        RunFacts facts = new RunFactsBuilder()
            .AtSegment(0)
            .AfterSeconds(100f)
            .WithBiomeSplits(new BiomeSplit(Biome.BiomeType.Shore, 100f, IsCurrent: true))
            .WithBiomeMedians(medians)
            .Build();

        // when
        float? eta = RunEta.Estimate(facts);

        // then
        Assert.Null(eta);
    }

    [Fact]
    public void No_eta_before_the_timeline_starts()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(2f).WithBiomeMedians(EveryBiome).Build();

        // when
        float? eta = RunEta.Estimate(facts);

        // then
        Assert.Null(eta);
    }
}

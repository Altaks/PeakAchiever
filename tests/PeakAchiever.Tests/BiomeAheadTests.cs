using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BiomeAheadTests
{
    // The builder's default map: Shore, Tropics, Alpine, Volcano (two segments), Peak.

    [Fact]
    public void A_biome_two_stretches_ahead_counts_two()
    {
        // given the team is on the Shore
        RunFacts facts = new RunFactsBuilder().AtSegment(0).Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Alpine).Describe(facts);

        // then
        Assert.Equal(new BadgeDetail.BiomeAhead(Biome.BiomeType.Alpine, StretchesAhead: 2), detail);
    }

    [Fact]
    public void The_biome_the_team_is_in_counts_zero()
    {
        // given the team is in the second Volcano segment
        RunFacts facts = new RunFactsBuilder().AtSegment(4).Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Volcano).Describe(facts);

        // then
        Assert.Equal(new BadgeDetail.BiomeAhead(Biome.BiomeType.Volcano, StretchesAhead: 0), detail);
    }

    [Fact]
    public void Two_segments_of_one_biome_count_as_one_stretch()
    {
        // given the team is in the Alpine, the Volcano spans two segments before the Peak
        RunFacts facts = new RunFactsBuilder().AtSegment(2).Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Peak).Describe(facts);

        // then
        Assert.Equal(new BadgeDetail.BiomeAhead(Biome.BiomeType.Peak, StretchesAhead: 2), detail);
    }

    [Fact]
    public void Of_several_biomes_the_nearest_ahead_is_named()
    {
        // given a badge found in the Mesa or the Alpine, on a map with only the Alpine
        RunFacts facts = new RunFactsBuilder().AtSegment(1).Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Mesa, Biome.BiomeType.Alpine).Describe(facts);

        // then
        Assert.Equal(new BadgeDetail.BiomeAhead(Biome.BiomeType.Alpine, StretchesAhead: 1), detail);
    }

    [Fact]
    public void Nothing_is_described_once_the_biome_is_behind()
    {
        // given the team is past the Tropics
        RunFacts facts = new RunFactsBuilder().AtSegment(3).Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Tropics).Describe(facts);

        // then: the red cross says it instead
        Assert.Null(detail);
    }

    [Fact]
    public void Nothing_is_described_in_the_nadir()
    {
        // given
        RunFacts facts = new RunFactsBuilder().InNadir().Build();

        // when
        BadgeDetail? detail = new BiomeAheadSource(Biome.BiomeType.Peak).Describe(facts);

        // then
        Assert.Null(detail);
    }

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.AstronomyBadge)]
    [InlineData(ACHIEVEMENTTYPE.NomadBadge)]
    [InlineData(ACHIEVEMENTTYPE.ArboristBadge)]
    public void Biome_badges_tell_how_far_their_biome_is(ACHIEVEMENTTYPE badge)
    {
        // given a map that holds every biome these badges need, the team on the Shore
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Tropics, Biome.BiomeType.Mesa, Biome.BiomeType.Volcano, Biome.BiomeType.Peak)
            .AtSegment(0)
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(badge).Detail(facts);

        // then
        Assert.IsType<BadgeDetail.BiomeAhead>(detail);
    }
}

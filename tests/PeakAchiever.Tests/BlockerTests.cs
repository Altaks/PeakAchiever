using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BlockerTests
{
    [Fact]
    public void InBiome_blocks_when_the_biome_is_not_on_this_map()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Mesa).FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.BiomeAbsent(Biome.BiomeType.Mesa), reason);
    }

    [Fact]
    public void InBiome_allows_a_biome_still_ahead()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AtSegment(0).Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Alpine).FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void InBiome_allows_the_biome_the_team_is_in()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AtSegment(2).Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Alpine).FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void InBiome_blocks_once_the_team_has_moved_past_the_biome()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AtSegment(3).Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Alpine).FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.BiomeLeft(Biome.BiomeType.Alpine), reason);
    }

    [Fact]
    public void InBiome_allows_a_biome_spanning_the_current_segment()
    {
        // given the volcano covers segments 3 and 4, and the team is in the second one
        RunFacts facts = new RunFactsBuilder().AtSegment(4).Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Volcano).FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void BiomeOnMap_blocks_when_the_biome_is_not_on_this_map()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();

        // when
        UnattainableReason? reason = new BiomeOnMap(Biome.BiomeType.Roots).FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.BiomeAbsent(Biome.BiomeType.Roots), reason);
    }

    [Fact]
    public void BiomeOnMap_still_allows_a_biome_already_left()
    {
        // given the game grants area badges once the next area is reached
        RunFacts facts = new RunFactsBuilder().AtSegment(3).Build();

        // when
        UnattainableReason? reason = new BiomeOnMap(Biome.BiomeType.Alpine).FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void InBiome_with_several_biomes_blocks_once_every_biome_on_the_map_is_left()
    {
        // given capybaras live in the Alpine or the Mesa; this map has the Alpine, already left
        RunFacts facts = new RunFactsBuilder().AtSegment(3).Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Alpine, Biome.BiomeType.Mesa).FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.BiomeLeft(Biome.BiomeType.Alpine), reason);
    }

    [Fact]
    public void InBiome_with_several_biomes_reports_all_of_them_when_none_is_on_the_map()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Mesa, Biome.BiomeType.Roots).FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.BiomeAbsent(Biome.BiomeType.Mesa, Biome.BiomeType.Roots), reason);
    }

    [Fact]
    public void InBiome_with_several_biomes_allows_the_one_the_team_is_in()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Roots, Biome.BiomeType.Mesa, Biome.BiomeType.Peak)
            .AtSegment(2)
            .Build();

        // when
        UnattainableReason? reason = new InBiome(Biome.BiomeType.Alpine, Biome.BiomeType.Mesa).FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void RunValueCeiling_allows_a_value_exactly_at_the_ceiling()
    {
        // given the game grants Cool Cucumber at <= 10 % heat
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.1f).Build();
        var ceiling = new RunValueCeiling(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.1f, BrokenCondition.TooMuchHeat);

        // when
        UnattainableReason? reason = ceiling.FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Fact]
    public void RunValueCeiling_blocks_a_value_above_the_ceiling()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.FallDamageTaken, 0.01f).Build();
        var ceiling = new RunValueCeiling(RUNBASEDVALUETYPE.FallDamageTaken, 0f, BrokenCondition.TookFallDamage);

        // when
        UnattainableReason? reason = ceiling.FindBlock(facts);

        // then
        Assert.Equal(new UnattainableReason.ConditionBroken(BrokenCondition.TookFallDamage), reason);
    }

    [Fact]
    public void RunValueCeiling_treats_a_missing_counter_as_zero()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();
        var ceiling = new RunValueCeiling(RUNBASEDVALUETYPE.TimesPassedOut, 0f, BrokenCondition.PassedOut);

        // when
        UnattainableReason? reason = ceiling.FindBlock(facts);

        // then
        Assert.Null(reason);
    }

    [Theory]
    [InlineData(3600f, false)]
    [InlineData(3600.5f, true)]
    public void RunDurationCeiling_blocks_only_past_the_limit(float seconds, bool blocked)
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(seconds).Build();

        // when
        UnattainableReason? reason = new RunDurationCeiling(3600f).FindBlock(facts);

        // then
        Assert.Equal(blocked ? new UnattainableReason.ConditionBroken(BrokenCondition.RunTooLong) : null, reason);
    }
}

using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BadgeCompatibilityTests
{
    // The two layouts PEAK 2.4.c picks from (MapBaker.selectedBiomes, 25 levels).
    private static readonly BadgeCompatibility TwoLayouts = new(
        [
            [Biome.BiomeType.Shore, Biome.BiomeType.Tropics, Biome.BiomeType.Alpine, Biome.BiomeType.Volcano],
            [Biome.BiomeType.Shore, Biome.BiomeType.Roots, Biome.BiomeType.Mesa, Biome.BiomeType.Swamp],
        ]
    );

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.TrailblazerBadge, ACHIEVEMENTTYPE.ForestryBadge)]
    [InlineData(ACHIEVEMENTTYPE.BundledUpBadge, ACHIEVEMENTTYPE.CoolCucumberBadge)]
    [InlineData(ACHIEVEMENTTYPE.VolcanologyBadge, ACHIEVEMENTTYPE.BellringerBadge)]
    [InlineData(ACHIEVEMENTTYPE.AlpinistBadge, ACHIEVEMENTTYPE.AstronomyBadge)]
    public void Badges_whose_biomes_no_map_holds_together_conflict(ACHIEVEMENTTYPE first, ACHIEVEMENTTYPE second)
    {
        // when
        bool compatible = TwoLayouts.CanShareARun(first, second);

        // then
        Assert.False(compatible);
    }

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.TrailblazerBadge, ACHIEVEMENTTYPE.AlpinistBadge)]
    [InlineData(ACHIEVEMENTTYPE.BeachcomberBadge, ACHIEVEMENTTYPE.ForestryBadge)]
    [InlineData(ACHIEVEMENTTYPE.AnimalSerenadingBadge, ACHIEVEMENTTYPE.TrailblazerBadge)]
    [InlineData(ACHIEVEMENTTYPE.AnimalSerenadingBadge, ACHIEVEMENTTYPE.ForestryBadge)]
    [InlineData(ACHIEVEMENTTYPE.ArboristBadge, ACHIEVEMENTTYPE.CoolCucumberBadge)]
    [InlineData(ACHIEVEMENTTYPE.LoneWolfBadge, ACHIEVEMENTTYPE.ClutchBadge)]
    [InlineData(ACHIEVEMENTTYPE.SpeedClimberBadge, ACHIEVEMENTTYPE.ForestryBadge)]
    public void Badges_some_map_allows_together_are_compatible(ACHIEVEMENTTYPE first, ACHIEVEMENTTYPE second)
    {
        // when
        bool compatible = TwoLayouts.CanShareARun(first, second);

        // then
        Assert.True(compatible);
    }

    [Fact]
    public void A_biome_the_level_table_never_lists_rules_nothing_out()
    {
        // given
        var withoutSwamp = new BadgeCompatibility([[Biome.BiomeType.Shore, Biome.BiomeType.Tropics]]);

        // when
        bool compatible = withoutSwamp.CanShareARun(ACHIEVEMENTTYPE.TrailblazerBadge, ACHIEVEMENTTYPE.WandererBadge);

        // then
        Assert.True(compatible);
    }

    [Fact]
    public void An_empty_level_table_rules_nothing_out()
    {
        // given
        var noTable = new BadgeCompatibility([]);

        // when
        bool compatible = noTable.CanShareARun(ACHIEVEMENTTYPE.TrailblazerBadge, ACHIEVEMENTTYPE.ForestryBadge);

        // then
        Assert.True(compatible);
    }

    [Fact]
    public void The_first_pinned_badge_in_conflict_is_named()
    {
        // given
        ACHIEVEMENTTYPE[] pinned = [ACHIEVEMENTTYPE.PeakBadge, ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.ForestryBadge];

        // when
        ACHIEVEMENTTYPE? conflict = TwoLayouts.FirstConflict(ACHIEVEMENTTYPE.AlpinistBadge, pinned);

        // then
        Assert.Equal(ACHIEVEMENTTYPE.NomadBadge, conflict);
    }

    [Fact]
    public void No_conflict_among_compatible_pins()
    {
        // when
        ACHIEVEMENTTYPE? conflict = TwoLayouts.FirstConflict(ACHIEVEMENTTYPE.AlpinistBadge, [ACHIEVEMENTTYPE.TrailblazerBadge]);

        // then
        Assert.Null(conflict);
    }
}

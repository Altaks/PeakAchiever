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
    private static readonly Biome.BiomeType[] TropicsMap =
    [
        Biome.BiomeType.Shore,
        Biome.BiomeType.Tropics,
        Biome.BiomeType.Alpine,
        Biome.BiomeType.Volcano,
        Biome.BiomeType.Peak,
    ];

    [Fact]
    public void A_badge_needing_a_biome_the_map_lacks_names_that_biome()
    {
        // when
        IReadOnlyCollection<Biome.BiomeType> missing = TwoLayouts.MissingOn(ACHIEVEMENTTYPE.ForestryBadge, TropicsMap);

        // then
        Assert.Equal([Biome.BiomeType.Roots], missing);
    }

    [Fact]
    public void A_badge_that_accepts_either_biome_names_both_when_the_map_has_neither()
    {
        // given
        Biome.BiomeType[] shoreOnly = [Biome.BiomeType.Shore];

        // when
        IReadOnlyCollection<Biome.BiomeType> missing = TwoLayouts.MissingOn(ACHIEVEMENTTYPE.ArboristBadge, shoreOnly);

        // then
        Assert.Equal([Biome.BiomeType.Tropics, Biome.BiomeType.Roots], missing);
    }

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.TrailblazerBadge)]
    [InlineData(ACHIEVEMENTTYPE.ArboristBadge)]
    [InlineData(ACHIEVEMENTTYPE.BalloonBadge)]
    public void A_badge_the_map_allows_misses_nothing(ACHIEVEMENTTYPE badge)
    {
        // when
        IReadOnlyCollection<Biome.BiomeType> missing = TwoLayouts.MissingOn(badge, TropicsMap);

        // then
        Assert.Empty(missing);
    }

    [Fact]
    public void A_biome_the_level_table_never_lists_is_never_missing()
    {
        // given
        var withoutSwamp = new BadgeCompatibility([[Biome.BiomeType.Shore, Biome.BiomeType.Tropics]]);

        // when
        IReadOnlyCollection<Biome.BiomeType> missing = withoutSwamp.MissingOn(ACHIEVEMENTTYPE.WandererBadge, TropicsMap);

        // then
        Assert.Empty(missing);
    }
}

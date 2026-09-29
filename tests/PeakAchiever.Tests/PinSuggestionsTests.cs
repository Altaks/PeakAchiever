using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class PinSuggestionsTests
{
    private static readonly BadgeCompatibility TwoLayouts = new(
        [
            [Biome.BiomeType.Shore, Biome.BiomeType.Tropics, Biome.BiomeType.Alpine, Biome.BiomeType.Volcano],
            [Biome.BiomeType.Shore, Biome.BiomeType.Roots, Biome.BiomeType.Mesa, Biome.BiomeType.Swamp],
        ]
    );

    private static readonly Biome.BiomeType[] RootsMap =
    [
        Biome.BiomeType.Shore,
        Biome.BiomeType.Roots,
        Biome.BiomeType.Mesa,
        Biome.BiomeType.Swamp,
        Biome.BiomeType.Peak,
    ];

    [Fact]
    public void Badges_of_todays_biomes_come_first_then_clean_runs_then_the_rest()
    {
        // given, in the game's badge order
        ACHIEVEMENTTYPE[] unearned =
        [
            ACHIEVEMENTTYPE.CookingBadge,
            ACHIEVEMENTTYPE.BalloonBadge,
            ACHIEVEMENTTYPE.NomadBadge,
            ACHIEVEMENTTYPE.TrailblazerBadge,
            ACHIEVEMENTTYPE.ForestryBadge,
        ];

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For(unearned, pinned: [], freeSlots: 4, RootsMap, TwoLayouts);

        // then: Trailblazer needs the Tropics, not on this map
        Assert.Equal(
            [ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.ForestryBadge, ACHIEVEMENTTYPE.BalloonBadge, ACHIEVEMENTTYPE.CookingBadge],
            suggested
        );
    }

    [Fact]
    public void No_more_suggestions_than_free_slots()
    {
        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For(
            [ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.ForestryBadge, ACHIEVEMENTTYPE.BalloonBadge],
            pinned: [],
            freeSlots: 2,
            RootsMap,
            TwoLayouts
        );

        // then
        Assert.Equal([ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.ForestryBadge], suggested);
    }

    [Fact]
    public void Pinned_badges_are_not_suggested_again()
    {
        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For(
            [ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.BalloonBadge],
            pinned: [ACHIEVEMENTTYPE.NomadBadge],
            freeSlots: 3,
            RootsMap,
            TwoLayouts
        );

        // then
        Assert.Equal([ACHIEVEMENTTYPE.BalloonBadge], suggested);
    }

    [Fact]
    public void A_badge_in_conflict_with_a_pin_is_not_suggested()
    {
        // given: a pin the map does not allow still rules out what it conflicts with
        ACHIEVEMENTTYPE[] pinned = [ACHIEVEMENTTYPE.TrailblazerBadge];

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For(
            [ACHIEVEMENTTYPE.NomadBadge, ACHIEVEMENTTYPE.BalloonBadge],
            pinned,
            freeSlots: 3,
            RootsMap,
            TwoLayouts
        );

        // then
        Assert.Equal([ACHIEVEMENTTYPE.BalloonBadge], suggested);
    }

    [Fact]
    public void No_suggestion_without_a_free_slot()
    {
        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For([ACHIEVEMENTTYPE.NomadBadge], pinned: [], freeSlots: 0, RootsMap, TwoLayouts);

        // then
        Assert.Empty(suggested);
    }

    [Fact]
    public void A_badge_the_map_cannot_hold_is_not_suggested()
    {
        // when
        IReadOnlyList<ACHIEVEMENTTYPE> suggested = PinSuggestions.For(
            [ACHIEVEMENTTYPE.TrailblazerBadge, ACHIEVEMENTTYPE.BalloonBadge],
            pinned: [],
            freeSlots: 3,
            RootsMap,
            TwoLayouts
        );

        // then
        Assert.Equal([ACHIEVEMENTTYPE.BalloonBadge], suggested);
    }
}

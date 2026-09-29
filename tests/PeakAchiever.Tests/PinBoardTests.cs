using PeakAchiever.Pinning;
using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class PinBoardTests
{
    [Fact]
    public void Toggle_pins_an_unearned_badge_at_the_end()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge], capacity: 5);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.KnotTyingBadge, isEarned: false, BadgeCompatibility.Unconstrained);

        // then
        Assert.Equal(PinToggleOutcome.Pinned, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_unpins_a_pinned_badge()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], capacity: 5);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.CookingBadge, isEarned: false, BadgeCompatibility.Unconstrained);

        // then
        Assert.Equal(PinToggleOutcome.Unpinned, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.KnotTyingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_rejects_a_new_pin_when_the_board_is_full()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], capacity: 2);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.BalloonBadge, isEarned: false, BadgeCompatibility.Unconstrained);

        // then
        Assert.Equal(PinToggleOutcome.RejectedBoardFull, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_still_unpins_when_the_board_is_over_a_lowered_capacity()
    {
        // given the player lowered the limit below the current pin count
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], capacity: 5)
        {
            Capacity = 1,
        };

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.KnotTyingBadge, isEarned: false, BadgeCompatibility.Unconstrained);

        // then
        Assert.Equal(PinToggleOutcome.Unpinned, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_pins_an_earned_badge_to_help_an_ally()
    {
        // given
        var board = new PinBoard([], capacity: 5);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.PeakBadge, isEarned: true, BadgeCompatibility.Unconstrained);

        // then
        Assert.Equal(PinToggleOutcome.Pinned, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.PeakBadge], board.Pins);
        Assert.True(board.IsForAlly(ACHIEVEMENTTYPE.PeakBadge));
    }

    [Fact]
    public void DropEarned_removes_only_earned_badges()
    {
        // given
        var board = new PinBoard(
            [ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.ForagingBadge, ACHIEVEMENTTYPE.KnotTyingBadge],
            capacity: 5
        );

        // when
        bool changed = board.DropEarned(badge => badge == ACHIEVEMENTTYPE.ForagingBadge);

        // then
        Assert.True(changed);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.KnotTyingBadge], board.Pins);
    }

    [Fact]
    public void DropEarned_reports_no_change_when_nothing_was_earned()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge], capacity: 5);

        // when
        bool changed = board.DropEarned(_ => false);

        // then
        Assert.False(changed);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_rejects_a_badge_no_map_allows_with_a_pinned_one()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.TrailblazerBadge], capacity: 5);
        var twoLayouts = new BadgeCompatibility(
            [
                [Biome.BiomeType.Shore, Biome.BiomeType.Tropics, Biome.BiomeType.Alpine, Biome.BiomeType.Volcano],
                [Biome.BiomeType.Shore, Biome.BiomeType.Roots, Biome.BiomeType.Mesa, Biome.BiomeType.Swamp],
            ]
        );

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.ForestryBadge, isEarned: false, twoLayouts);

        // then
        Assert.Equal(PinToggleOutcome.RejectedConflict, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.TrailblazerBadge], board.Pins);
    }

    [Fact]
    public void DropEarned_keeps_the_badges_pinned_to_help_an_ally()
    {
        // given: Foraging was pinned while already earned, Cooking was earned after being pinned
        var board = new PinBoard(
            [ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.ForagingBadge],
            capacity: 5,
            pinnedForAllies: [ACHIEVEMENTTYPE.ForagingBadge]
        );

        // when
        bool changed = board.DropEarned(_ => true);

        // then
        Assert.True(changed);
        Assert.Equal([ACHIEVEMENTTYPE.ForagingBadge], board.Pins);
    }

    [Fact]
    public void Unpinning_a_badge_pinned_for_an_ally_forgets_that_it_was()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.PeakBadge], capacity: 5, pinnedForAllies: [ACHIEVEMENTTYPE.PeakBadge]);

        // when
        board.Toggle(ACHIEVEMENTTYPE.PeakBadge, isEarned: true, BadgeCompatibility.Unconstrained);

        // then
        Assert.Empty(board.Pins);
        Assert.False(board.IsForAlly(ACHIEVEMENTTYPE.PeakBadge));
    }

    [Fact]
    public void A_badge_pinned_before_it_was_earned_is_not_for_an_ally()
    {
        // given
        var board = new PinBoard([], capacity: 5);

        // when
        board.Toggle(ACHIEVEMENTTYPE.CookingBadge, isEarned: false, BadgeCompatibility.Unconstrained);

        // then
        Assert.False(board.IsForAlly(ACHIEVEMENTTYPE.CookingBadge));
    }
}

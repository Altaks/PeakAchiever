using PeakAchiever.Pinning;

namespace PeakAchiever.Tests;

public class PinBoardTests
{
    [Fact]
    public void Toggle_pins_an_unearned_badge_at_the_end()
    {
        // given
        var board = new PinBoard([ACHIEVEMENTTYPE.CookingBadge], capacity: 5);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.KnotTyingBadge, isEarned: false);

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
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.CookingBadge, isEarned: false);

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
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.BalloonBadge, isEarned: false);

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
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.KnotTyingBadge, isEarned: false);

        // then
        Assert.Equal(PinToggleOutcome.Unpinned, outcome);
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge], board.Pins);
    }

    [Fact]
    public void Toggle_rejects_an_earned_badge()
    {
        // given
        var board = new PinBoard([], capacity: 5);

        // when
        PinToggleOutcome outcome = board.Toggle(ACHIEVEMENTTYPE.PeakBadge, isEarned: true);

        // then
        Assert.Equal(PinToggleOutcome.RejectedAlreadyEarned, outcome);
        Assert.Empty(board.Pins);
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
}

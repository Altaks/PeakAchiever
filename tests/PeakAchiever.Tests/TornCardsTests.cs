using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class TornCardsTests
{
    private static readonly TrackedStatus Doable = new TrackedStatus.Attainable(null);
    private static readonly TrackedStatus Crossed = new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.RunLost));
    private static readonly TrackedStatus Earned = new TrackedStatus.Achieved(null);

    private static TrackedBadge Tracked(ACHIEVEMENTTYPE badge, TrackedStatus status) => new(badge, BadgeRules.For(badge), status, Detail: null);

    [Fact]
    public void A_card_that_turns_impossible_while_followed_tears()
    {
        // given
        var torn = new TornCards();
        torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable)]);

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]);

        // then
        Assert.Equal([ACHIEVEMENTTYPE.ForagingBadge], tearing);
    }

    [Fact]
    public void A_card_already_impossible_when_first_seen_does_not_tear()
    {
        // given
        var torn = new TornCards();

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]);

        // then
        Assert.Empty(tearing);
    }

    [Fact]
    public void A_torn_card_tears_only_once()
    {
        // given
        var torn = new TornCards();
        torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable)]);
        torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]);

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]);

        // then
        Assert.Empty(tearing);
    }

    [Fact]
    public void Several_cards_tear_in_the_pins_order()
    {
        // given
        var torn = new TornCards();
        torn.Update([Tracked(ACHIEVEMENTTYPE.PeakBadge, Earned), Tracked(ACHIEVEMENTTYPE.CookingBadge, Doable), Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable)]);

        // when
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = torn.Update(
            [Tracked(ACHIEVEMENTTYPE.PeakBadge, Earned), Tracked(ACHIEVEMENTTYPE.CookingBadge, Crossed), Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]
        );

        // then
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.ForagingBadge], tearing);
    }

    [Fact]
    public void Leaving_the_run_forgets_every_card()
    {
        // given
        var torn = new TornCards();
        torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable)]);
        torn.Update([]);

        // when: the next run starts with the badge already impossible
        IReadOnlyList<ACHIEVEMENTTYPE> tearing = torn.Update([Tracked(ACHIEVEMENTTYPE.ForagingBadge, Crossed)]);

        // then
        Assert.Empty(tearing);
    }

    [Fact]
    public void Torn_cards_go_last_in_the_order_they_tore()
    {
        // given
        TrackedBadge[] tracked =
        [
            Tracked(ACHIEVEMENTTYPE.PeakBadge, Earned),
            Tracked(ACHIEVEMENTTYPE.CookingBadge, Crossed),
            Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable),
            Tracked(ACHIEVEMENTTYPE.BalloonBadge, Crossed),
        ];

        // when: Balloon tore first, then Cooking
        IReadOnlyList<TrackedBadge> column = TornCards.Arrange(tracked, [ACHIEVEMENTTYPE.BalloonBadge, ACHIEVEMENTTYPE.CookingBadge]);

        // then
        Assert.Equal(
            [ACHIEVEMENTTYPE.PeakBadge, ACHIEVEMENTTYPE.ForagingBadge, ACHIEVEMENTTYPE.BalloonBadge, ACHIEVEMENTTYPE.CookingBadge],
            column.Select(badge => badge.Badge)
        );
    }

    [Fact]
    public void A_torn_badge_no_longer_pinned_is_left_out()
    {
        // when
        IReadOnlyList<TrackedBadge> column = TornCards.Arrange([Tracked(ACHIEVEMENTTYPE.PeakBadge, Earned)], [ACHIEVEMENTTYPE.CookingBadge]);

        // then
        Assert.Equal([ACHIEVEMENTTYPE.PeakBadge], column.Select(badge => badge.Badge));
    }
}

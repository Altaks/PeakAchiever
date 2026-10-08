using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class FoldedCardsTests
{
    private static readonly TrackedStatus Doable = new TrackedStatus.Attainable(null);
    private static readonly TrackedStatus Earned = new TrackedStatus.Achieved(null);
    private static readonly TrackedStatus Crossed = new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.RunLost));

    private static TrackedBadge Tracked(ACHIEVEMENTTYPE badge, TrackedStatus status, bool forAlly = false) =>
        new(badge, BadgeRules.For(badge), status, Detail: null, forAlly);

    [Fact]
    public void A_card_earned_while_followed_stays_whole_for_a_moment()
    {
        // given
        var folded = new FoldedCards();
        folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Doable)], now: 10f);

        // when: earned, then checked just before the delay is up
        folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned)], now: 20f);
        IReadOnlyCollection<ACHIEVEMENTTYPE> shown = folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned)], now: 20f + FoldedCards.FoldDelaySeconds - 0.1f);

        // then
        Assert.Empty(shown);
    }

    [Fact]
    public void A_card_earned_while_followed_folds_once_the_delay_is_up()
    {
        // given
        var folded = new FoldedCards();
        folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Doable)], now: 10f);
        folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned)], now: 20f);

        // when
        IReadOnlyCollection<ACHIEVEMENTTYPE> shown = folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned)], now: 20f + FoldedCards.FoldDelaySeconds);

        // then
        Assert.Equal([ACHIEVEMENTTYPE.PlundererBadge], shown);
    }

    [Fact]
    public void A_card_already_earned_when_first_seen_shows_folded()
    {
        // given
        var folded = new FoldedCards();

        // when
        IReadOnlyCollection<ACHIEVEMENTTYPE> shown = folded.Update([Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned)], now: 5f);

        // then
        Assert.Equal([ACHIEVEMENTTYPE.PlundererBadge], shown);
    }

    [Fact]
    public void A_badge_pinned_for_an_ally_never_folds()
    {
        // given
        var folded = new FoldedCards();

        // when
        IReadOnlyCollection<ACHIEVEMENTTYPE> shown = folded.Update([Tracked(ACHIEVEMENTTYPE.LoneWolfBadge, Earned, forAlly: true)], now: 999f);

        // then
        Assert.Empty(shown);
    }

    [Fact]
    public void Earned_cards_sit_after_those_in_play_and_before_torn_ones()
    {
        // given
        TrackedBadge[] tracked =
        [
            Tracked(ACHIEVEMENTTYPE.KnotTyingBadge, Earned),
            Tracked(ACHIEVEMENTTYPE.BalloonBadge, Crossed),
            Tracked(ACHIEVEMENTTYPE.ForagingBadge, Doable),
            Tracked(ACHIEVEMENTTYPE.PlundererBadge, Earned),
        ];

        // when: Plunderer is still showing its check mark at full size
        IReadOnlyList<TrackedBadge> column = TornCards.Arrange(tracked, [ACHIEVEMENTTYPE.BalloonBadge], folded: [ACHIEVEMENTTYPE.KnotTyingBadge]);

        // then
        Assert.Equal(
            [ACHIEVEMENTTYPE.ForagingBadge, ACHIEVEMENTTYPE.PlundererBadge, ACHIEVEMENTTYPE.KnotTyingBadge, ACHIEVEMENTTYPE.BalloonBadge],
            column.Select(badge => badge.Badge)
        );
    }
}

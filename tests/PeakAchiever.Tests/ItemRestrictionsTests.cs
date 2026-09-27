using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class ItemRestrictionsTests
{
    private static TrackedBadge Tracked(ACHIEVEMENTTYPE badge, TrackedStatus status) =>
        new(badge, BadgeRules.For(badge), status);

    [Fact]
    public void Holding_naturalist_forbids_packaged_food()
    {
        // given
        TrackedBadge[] tracked = [Tracked(ACHIEVEMENTTYPE.NaturalistBadge, new TrackedStatus.Holding())];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.PackagedFood, forbidden);
    }

    [Fact]
    public void Holding_leave_no_trace_forbids_permanent_objects()
    {
        // given
        TrackedBadge[] tracked = [Tracked(ACHIEVEMENTTYPE.LeaveNoTraceBadge, new TrackedStatus.Holding())];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.PlacesPermanentObject, forbidden);
    }

    [Fact]
    public void Both_badges_holding_forbid_both_kinds()
    {
        // given
        TrackedBadge[] tracked =
        [
            Tracked(ACHIEVEMENTTYPE.NaturalistBadge, new TrackedStatus.Holding()),
            Tracked(ACHIEVEMENTTYPE.LeaveNoTraceBadge, new TrackedStatus.Holding()),
        ];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.PackagedFood | ItemTraits.PlacesPermanentObject, forbidden);
    }

    [Fact]
    public void Broken_badge_frees_its_items()
    {
        // given
        var broken = new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.AtePackagedFood));
        TrackedBadge[] tracked = [Tracked(ACHIEVEMENTTYPE.NaturalistBadge, broken)];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.None, forbidden);
    }

    [Fact]
    public void Earned_badge_frees_its_items()
    {
        // given
        TrackedBadge[] tracked = [Tracked(ACHIEVEMENTTYPE.LeaveNoTraceBadge, new TrackedStatus.Achieved(null))];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.None, forbidden);
    }

    [Fact]
    public void Clean_run_without_item_rule_forbids_nothing()
    {
        // given the balloon badge is about fall damage, not items
        TrackedBadge[] tracked = [Tracked(ACHIEVEMENTTYPE.BalloonBadge, new TrackedStatus.Holding())];

        // when
        ItemTraits forbidden = ItemRestrictions.ForbiddenBy(tracked);

        // then
        Assert.Equal(ItemTraits.None, forbidden);
    }
}

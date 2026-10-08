using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BadgeNeedsTests
{
    [Fact]
    public void Twenty_four_karat_takes_the_ancient_idol_only()
    {
        // when
        BadgeNeed? need = BadgeNeeds.For(ACHIEVEMENTTYPE.TwentyFourKaratBadge);

        // then: the one item tagged GoldenIdol, which Lava.ValidIdolSacrifice requires
        Assert.Equal(new BadgeNeed(NeedKind.All, [new NeededItem("Ancient Idol")], []).Items, need!.Items);
        Assert.Equal(NeedKind.All, need.Kind);
    }

    [Fact]
    public void Every_listed_need_names_items_in_a_positive_count()
    {
        // when
        NeededItem[] all = BadgeNeeds.Needs.Values.SelectMany(need => need.Items.Concat(need.Helps)).ToArray();

        // then
        Assert.All(all, item => Assert.True(item.Count >= 1 && item.ItemName.Length > 0, item.ToString()));
    }

    [Fact]
    public void Every_listed_need_shows_something()
    {
        // when
        ACHIEVEMENTTYPE[] empty = BadgeNeeds.Needs.Where(need => need.Value.Items.Count + need.Value.Helps.Count == 0).Select(need => need.Key).ToArray();

        // then
        Assert.Empty(empty);
    }

    [Fact]
    public void A_badge_with_no_listed_item_needs_nothing()
    {
        // when
        BadgeNeed? need = BadgeNeeds.For(ACHIEVEMENTTYPE.PeakBadge);

        // then
        Assert.Null(need);
    }
}

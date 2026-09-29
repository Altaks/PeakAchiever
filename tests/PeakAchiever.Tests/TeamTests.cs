using PeakAchiever.Team;

namespace PeakAchiever.Tests;

public class BadgeNamesTests
{
    [Fact]
    public void Badges_read_back_from_their_names_in_order()
    {
        // given
        ACHIEVEMENTTYPE[] badges = [ACHIEVEMENTTYPE.BellringerBadge, ACHIEVEMENTTYPE.CoolCucumberBadge];

        // when
        BadgeNames.Parsed read = BadgeNames.Parse(BadgeNames.Join(badges));

        // then
        Assert.Equal(badges, read.Badges);
        Assert.Empty(read.Unknown);
    }

    [Fact]
    public void An_empty_text_holds_no_badge()
    {
        // when
        BadgeNames.Parsed read = BadgeNames.Parse("");

        // then
        Assert.Empty(read.Badges);
        Assert.Empty(read.Unknown);
    }

    [Theory]
    [InlineData("BadgeFromANewerGame")]
    [InlineData("999")]
    [InlineData("1")]
    [InlineData("NONE")]
    public void A_name_this_game_does_not_know_is_set_aside(string name)
    {
        // when
        BadgeNames.Parsed read = BadgeNames.Parse($"PeakBadge,{name}");

        // then
        Assert.Equal([ACHIEVEMENTTYPE.PeakBadge], read.Badges);
        Assert.Equal([name], read.Unknown);
    }

    [Fact]
    public void A_badge_named_twice_counts_once()
    {
        // when
        BadgeNames.Parsed read = BadgeNames.Parse("PeakBadge, PeakBadge");

        // then
        Assert.Equal([ACHIEVEMENTTYPE.PeakBadge], read.Badges);
    }
}

public class ScoutsTests
{
    private static Scout With(string name, params ACHIEVEMENTTYPE[] earned) => new(name, IsLocal: false, earned.ToHashSet());

    private static Scout WithoutTheMod(string name) => new(name, IsLocal: false, Earned: null);

    [Fact]
    public void A_badge_every_scout_with_the_mod_has_is_earned_by_all()
    {
        // given
        Scout[] scouts = [With("Mika", ACHIEVEMENTTYPE.PeakBadge), With("Jo", ACHIEVEMENTTYPE.PeakBadge), WithoutTheMod("Tom")];

        // when
        bool all = Scouts.EarnedByAll(ACHIEVEMENTTYPE.PeakBadge, scouts);

        // then
        Assert.True(all);
    }

    [Fact]
    public void One_scout_without_it_is_enough_to_keep_it()
    {
        // given
        Scout[] scouts = [With("Mika", ACHIEVEMENTTYPE.PeakBadge), With("Jo")];

        // when
        bool all = Scouts.EarnedByAll(ACHIEVEMENTTYPE.PeakBadge, scouts);

        // then
        Assert.False(all);
    }

    [Fact]
    public void Nobody_known_proves_nothing()
    {
        // given
        Scout[] scouts = [WithoutTheMod("Tom"), WithoutTheMod("Léa")];

        // when
        bool all = Scouts.EarnedByAll(ACHIEVEMENTTYPE.PeakBadge, scouts);

        // then
        Assert.False(all);
    }
}

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

public class TeamRankingTests
{
    private static Scout With(string name, params ACHIEVEMENTTYPE[] earned) => new(name, IsLocal: false, earned.ToHashSet());

    private static readonly Scout[] Team =
    [
        With("Altaks", ACHIEVEMENTTYPE.CoolCucumberBadge, ACHIEVEMENTTYPE.NaturalistBadge, ACHIEVEMENTTYPE.PeakBadge),
        With("Mika", ACHIEVEMENTTYPE.NaturalistBadge, ACHIEVEMENTTYPE.PeakBadge),
        With("Jo", ACHIEVEMENTTYPE.PeakBadge),
        new("Tom", IsLocal: false, Earned: null),
    ];

    [Fact]
    public void Badges_nobody_has_come_first_then_the_most_missing()
    {
        // given, in the game's badge order
        ACHIEVEMENTTYPE[] candidates =
        [
            ACHIEVEMENTTYPE.NaturalistBadge,
            ACHIEVEMENTTYPE.BellringerBadge,
            ACHIEVEMENTTYPE.CoolCucumberBadge,
            ACHIEVEMENTTYPE.WebSecurityBadge,
        ];

        // when
        IReadOnlyList<TeamRow> rows = TeamRanking.Rank(candidates, Team);

        // then
        Assert.Equal(
            [ACHIEVEMENTTYPE.BellringerBadge, ACHIEVEMENTTYPE.WebSecurityBadge, ACHIEVEMENTTYPE.CoolCucumberBadge, ACHIEVEMENTTYPE.NaturalistBadge],
            rows.Select(row => row.Badge)
        );
    }

    [Fact]
    public void A_row_names_who_misses_it_and_counts_only_the_scouts_known()
    {
        // when
        TeamRow row = TeamRanking.Rank([ACHIEVEMENTTYPE.NaturalistBadge], Team).Single();

        // then
        Assert.Equal(2, row.EarnedBy);
        Assert.Equal(3, row.Known);
        Assert.Equal(["Jo"], row.MissingFor);
        Assert.False(row.NobodyHasIt);
    }

    [Fact]
    public void A_badge_every_known_scout_has_is_left_out()
    {
        // when
        IReadOnlyList<TeamRow> rows = TeamRanking.Rank([ACHIEVEMENTTYPE.PeakBadge, ACHIEVEMENTTYPE.BellringerBadge], Team);

        // then
        Assert.Equal([ACHIEVEMENTTYPE.BellringerBadge], rows.Select(row => row.Badge));
    }

    [Fact]
    public void With_nobody_known_every_badge_is_listed_as_nobody_has_it()
    {
        // given
        Scout[] unknown = [new("Tom", IsLocal: false, Earned: null)];

        // when
        IReadOnlyList<TeamRow> rows = TeamRanking.Rank([ACHIEVEMENTTYPE.PeakBadge], unknown);

        // then
        Assert.True(rows.Single().NobodyHasIt);
    }
}


public class TrackedPinsTests
{
    [Fact]
    public void Team_pins_come_first_and_a_badge_in_both_lists_shows_once_as_a_team_pin()
    {
        // given
        ACHIEVEMENTTYPE[] team = [ACHIEVEMENTTYPE.DaredevilBadge, ACHIEVEMENTTYPE.PlundererBadge];
        ACHIEVEMENTTYPE[] own = [ACHIEVEMENTTYPE.SpeedClimberBadge, ACHIEVEMENTTYPE.PlundererBadge, ACHIEVEMENTTYPE.CookingBadge];

        // when
        IReadOnlyList<(ACHIEVEMENTTYPE Badge, bool ForTeam)> pins = TrackedPins.Merge(team, own);

        // then
        Assert.Equal(
            [
                (ACHIEVEMENTTYPE.DaredevilBadge, true),
                (ACHIEVEMENTTYPE.PlundererBadge, true),
                (ACHIEVEMENTTYPE.SpeedClimberBadge, false),
                (ACHIEVEMENTTYPE.CookingBadge, false),
            ],
            pins
        );
    }

    [Fact]
    public void Without_team_pins_the_own_pins_keep_their_order()
    {
        // when
        IReadOnlyList<(ACHIEVEMENTTYPE Badge, bool ForTeam)> pins = TrackedPins.Merge([], [ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.PeakBadge]);

        // then
        Assert.Equal([(ACHIEVEMENTTYPE.CookingBadge, false), (ACHIEVEMENTTYPE.PeakBadge, false)], pins);
    }

    [Fact]
    public void Missing_for_names_the_known_scouts_without_the_badge()
    {
        // given one scout has it, one does not, one runs no mod
        Scout[] scouts =
        [
            new("Mara", IsLocal: false, Earned: [ACHIEVEMENTTYPE.DaredevilBadge]),
            new("Teo", IsLocal: false, Earned: []),
            new("Ana", IsLocal: false, Earned: null),
        ];

        // when
        IReadOnlyList<string> missing = Scouts.MissingFor(ACHIEVEMENTTYPE.DaredevilBadge, scouts);

        // then
        Assert.Equal(["Teo"], missing);
    }
}

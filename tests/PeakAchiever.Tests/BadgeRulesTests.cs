using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BadgeRulesTests
{
    [Fact]
    public void Every_real_badge_has_a_rule_traced_to_the_game()
    {
        // given the game's own list of badges that count (ascents excluded)
        ACHIEVEMENTTYPE[] realBadges = Enum.GetValues<ACHIEVEMENTTYPE>()
            .Where(badge => badge != ACHIEVEMENTTYPE.NONE && AchievementManager.IsRealAchievement(badge))
            .ToArray();

        // when
        ACHIEVEMENTTYPE[] missing = realBadges.Where(badge => !BadgeRules.Rules.ContainsKey(badge)).ToArray();

        // then
        Assert.Empty(missing);
    }

    [Fact]
    public void Lifetime_stats_list_holds_exactly_the_stats_the_rules_measure()
    {
        // given
        STEAMSTATTYPE[] expected =
        [
            STEAMSTATTYPE.MealsCooked,
            STEAMSTATTYPE.MoraleBoosts,
            STEAMSTATTYPE.PitonsPlaced,
            STEAMSTATTYPE.PoisonHealed,
            STEAMSTATTYPE.HeightClimbed,
            STEAMSTATTYPE.TotalPagesRead,
            STEAMSTATTYPE.DamageBlockedByMilk,
        ];

        // when
        STEAMSTATTYPE[] actual = BadgeRules.LifetimeStats;

        // then
        Assert.Equal(expected.Order(), actual.Order());
    }

    [Fact]
    public void Unknown_badge_stays_doable_without_counter()
    {
        // given the NONE placeholder has no rule
        RunFacts facts = new RunFactsBuilder().AtSegment(5).AfterSeconds(9999f).Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.NONE).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(null), status);
    }

    [Fact]
    public void Knot_tying_reads_the_rope_placed_this_run_against_100_meters()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.RopePlaced, 42.5f).Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.KnotTyingBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(new Progress(42, 100, ProgressScope.ThisRun)), status);
    }

    [Fact]
    public void Cool_cucumber_breaks_above_ten_percent_heat()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Tropics, Biome.BiomeType.Mesa, Biome.BiomeType.Peak)
            .WithRunValue(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.11f)
            .Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.CoolCucumberBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(
            new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.TooMuchHeat)),
            status
        );
    }

    [Fact]
    public void Speed_climber_breaks_after_one_hour()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(3601f).Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(
            new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.RunTooLong)),
            status
        );
    }
    [Fact]
    public void Lone_wolf_holds_until_the_summit_whoever_climbs_along()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.LoneWolfBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Holding(), status);
    }
}

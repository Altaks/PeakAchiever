using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class BadgeRuleTests
{
    [Fact]
    public void Counted_rule_shows_run_progress_towards_the_target()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.RopePlaced, 37.8f).Build();
        BadgeRule rule = BadgeRule.Counted(new RunValueTarget(RUNBASEDVALUETYPE.RopePlaced, 100));

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(new Progress(37, 100, ProgressScope.ThisRun)), status);
    }

    [Fact]
    public void Lifetime_counter_reports_the_lifetime_scope()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithLifetimeStat(STEAMSTATTYPE.MealsCooked, 14).Build();
        BadgeRule rule = BadgeRule.Counted(new LifetimeStatTarget(STEAMSTATTYPE.MealsCooked, 20));

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(new Progress(14, 20, ProgressScope.Lifetime)), status);
    }

    [Fact]
    public void Collection_counter_counts_distinct_items_of_the_run()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithCollection(RunCollection.DifferentBerriesEaten, 3).Build();
        BadgeRule rule = BadgeRule.Counted(new RunCollectionTarget(RunCollection.DifferentBerriesEaten, 5));

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(new Progress(3, 5, ProgressScope.ThisRun)), status);
    }

    [Fact]
    public void Unlocked_badge_is_achieved_and_keeps_its_progress()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithCollection(RunCollection.DifferentBerriesEaten, 5).Build();
        BadgeRule rule = BadgeRule.Counted(new RunCollectionTarget(RunCollection.DifferentBerriesEaten, 5));

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: true);

        // then
        Assert.Equal(new TrackedStatus.Achieved(new Progress(5, 5, ProgressScope.ThisRun)), status);
    }

    [Fact]
    public void Unlocked_badge_wins_over_a_blocker()
    {
        // given the badge was earned in Mesa, then the team moved on
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Mesa, Biome.BiomeType.Peak)
            .AtSegment(2)
            .Build();
        BadgeRule rule = BadgeRule.OneOff(new InBiome(Biome.BiomeType.Mesa));

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: true);

        // then
        Assert.Equal(new TrackedStatus.Achieved(null), status);
    }

    [Fact]
    public void Clean_run_holds_while_nothing_is_broken()
    {
        // given
        RunFacts facts = new RunFactsBuilder().Build();
        BadgeRule rule = BadgeRule.CleanRun(
            new RunValueCeiling(RUNBASEDVALUETYPE.FallDamageTaken, 0f, BrokenCondition.TookFallDamage)
        );

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Holding(), status);
    }

    [Fact]
    public void Clean_run_is_unattainable_once_broken()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.FallDamageTaken, 0.2f).Build();
        BadgeRule rule = BadgeRule.CleanRun(
            new RunValueCeiling(RUNBASEDVALUETYPE.FallDamageTaken, 0f, BrokenCondition.TookFallDamage)
        );

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(
            new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.TookFallDamage)),
            status
        );
    }

    [Fact]
    public void First_blocker_in_declaration_order_is_reported()
    {
        // given the Mesa is absent and the heat counter is also over its ceiling
        RunFacts facts = new RunFactsBuilder().WithRunValue(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.5f).Build();
        BadgeRule rule = BadgeRule.CleanRun(
            new InBiome(Biome.BiomeType.Mesa),
            new RunValueCeiling(RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.1f, BrokenCondition.TooMuchHeat)
        );

        // when
        TrackedStatus status = rule.Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(
            new TrackedStatus.Unattainable(new UnattainableReason.BiomeAbsent(Biome.BiomeType.Mesa)),
            status
        );
    }

    [Fact]
    public void One_off_rule_without_blockers_stays_attainable_with_no_counter()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AtSegment(5).Build();

        // when
        TrackedStatus status = BadgeRule.OneOff().Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(null), status);
    }

    [Theory]
    [InlineData(0, 20, 0f)]
    [InlineData(14, 20, 0.7f)]
    [InlineData(25, 20, 1f)]
    public void Progress_fraction_is_clamped_to_one(int current, int target, float expected)
    {
        // given
        var progress = new Progress(current, target, ProgressScope.Lifetime);

        // when
        float fraction = progress.Fraction;

        // then
        Assert.Equal(expected, fraction, precision: 3);
    }
}

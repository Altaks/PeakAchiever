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
        RunFacts facts = new RunFactsBuilder().AtSegment(5).WithScouts(4).AfterSeconds(9999f).Build();

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
    public void Speed_climber_holds_with_the_run_time_against_one_hour()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(2533.7f).Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Holding(new Progress(2533, 3600, ProgressScope.ThisRun, ProgressUnit.Duration)), status);
    }

    [Fact]
    public void Speed_climber_details_the_run_clock_with_its_biome_splits()
    {
        // given
        BiomeSplit[] splits =
        [
            new(Biome.BiomeType.Shore, 492f, IsCurrent: false),
            new(Biome.BiomeType.Tropics, 1041.7f, IsCurrent: true),
        ];
        RunFacts facts = new RunFactsBuilder().AfterSeconds(1533.7f).WithBiomeSplits(splits).Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Detail(facts);

        // then
        Assert.Equal(new BadgeDetail.RunClock(1533.7f, [new(splits[0], null), new(splits[1], null)], EtaSeconds: null, EtaOverLimit: false), detail);
    }

    [Fact]
    public void Speed_climber_keeps_its_run_clock_once_broken()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(3891f).Build();
        BadgeRule rule = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge);

        // when
        BadgeDetail? detail = rule.Detail(facts);

        // then
        Assert.IsType<TrackedStatus.Unattainable>(rule.Evaluate(facts, isUnlocked: false));
        Assert.Equal(new BadgeDetail.RunClock(3891f, [], EtaSeconds: null, EtaOverLimit: false), detail);
    }

    [Fact]
    public void A_badge_with_no_detail_describes_nothing()
    {
        // given
        RunFacts facts = new RunFactsBuilder().AfterSeconds(100f).Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.BalloonBadge).Detail(facts);

        // then
        Assert.Null(detail);
    }

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.CoolCucumberBadge, RUNBASEDVALUETYPE.MaxHeatTakenInMesa, 0.04f, 4, 10)]
    [InlineData(ACHIEVEMENTTYPE.BundledUpBadge, RUNBASEDVALUETYPE.MaxColdTakenInAlpine, 0.2f, 20, 20)]
    [InlineData(ACHIEVEMENTTYPE.TreadLightlyBadge, RUNBASEDVALUETYPE.MaxSporesTakenInRoots, 0.126f, 13, 25)]
    public void Biome_rate_badges_hold_with_the_highest_rate_against_the_limit(
        ACHIEVEMENTTYPE badge,
        RUNBASEDVALUETYPE rate,
        float highest,
        int percent,
        int limitPercent
    )
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Roots, Biome.BiomeType.Alpine, Biome.BiomeType.Mesa)
            .WithRunValue(rate, highest)
            .Build();

        // when
        TrackedStatus status = BadgeRules.For(badge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Holding(new Progress(percent, limitPercent, ProgressScope.ThisRun, ProgressUnit.Percent)), status);
    }

    [Fact]
    public void Foraging_checks_off_the_berries_eaten_among_every_berry_of_the_game()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithCandidates(RunCollection.DifferentBerriesEaten, 3, 7, 12)
            .WithEaten(RunCollection.DifferentBerriesEaten, 12, 3)
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.ForagingBadge).Detail(facts);

        // then
        Assert.Equal(
            new BadgeDetail.Checklist([new ChecklistItem(3, Eaten: true, OnMap: true), new ChecklistItem(7, Eaten: false, OnMap: true), new ChecklistItem(12, Eaten: true, OnMap: true)]),
            detail
        );
    }

    [Theory]
    [InlineData(ACHIEVEMENTTYPE.ForagingBadge, RunCollection.DifferentBerriesEaten)]
    [InlineData(ACHIEVEMENTTYPE.AdvancedMycologyBadge, RunCollection.DifferentShroomBerriesEaten)]
    [InlineData(ACHIEVEMENTTYPE.MycologyBadge, RunCollection.DifferentNonToxicMushroomsEaten)]
    [InlineData(ACHIEVEMENTTYPE.GourmandBadge, RunCollection.GourmandDishesEaten)]
    // RunCollection is internal, so a public theory takes it as object.
    public void Each_eating_badge_lists_its_own_collection(ACHIEVEMENTTYPE badge, object listEaten)
    {
        // given
        var collection = (RunCollection)listEaten;
        RunFacts facts = new RunFactsBuilder().WithCandidates(collection, 42).WithEaten(collection, 42).Build();

        // when
        BadgeDetail? detail = BadgeRules.For(badge).Detail(facts);

        // then
        Assert.Equal(new BadgeDetail.Checklist([new ChecklistItem(42, Eaten: true, OnMap: true)]), detail);
    }

    [Fact]
    public void The_counter_counts_the_distinct_items_eaten()
    {
        // given
        RunFacts facts = new RunFactsBuilder().WithEaten(RunCollection.GourmandDishesEaten, 5, 9).Build();

        // when
        TrackedStatus status = BadgeRules.For(ACHIEVEMENTTYPE.GourmandBadge).Evaluate(facts, isUnlocked: false);

        // then
        Assert.Equal(new TrackedStatus.Attainable(new Progress(2, 4, ProgressScope.ThisRun)), status);
    }

    [Fact]
    public void Speed_climber_forecasts_the_finish_from_past_runs()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Peak)
            .AfterSeconds(300f)
            .WithBiomeSplits(new BiomeSplit(Biome.BiomeType.Shore, 300f, IsCurrent: true))
            .WithBiomeMedians(new Dictionary<Biome.BiomeType, float> { [Biome.BiomeType.Shore] = 500f, [Biome.BiomeType.Peak] = 90f })
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Detail(facts);

        // then
        Assert.Equal(590f, Assert.IsType<BadgeDetail.RunClock>(detail).EtaSeconds);
    }

    [Fact]
    public void Speed_climber_compares_each_biome_with_its_median()
    {
        // given
        var shore = new BiomeSplit(Biome.BiomeType.Shore, 460f, IsCurrent: false);
        RunFacts facts = new RunFactsBuilder()
            .AfterSeconds(600f)
            .WithBiomeSplits(shore, new BiomeSplit(Biome.BiomeType.Tropics, 140f, IsCurrent: true))
            .WithBiomeMedians(new Dictionary<Biome.BiomeType, float> { [Biome.BiomeType.Shore] = 500f })
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Detail(facts);

        // then
        Assert.Equal(new ComparedSplit(shore, -40f), Assert.IsType<BadgeDetail.RunClock>(detail).Splits[0]);
    }

    [Theory]
    [InlineData(3500f, false)]
    [InlineData(3700f, true)]
    public void Speed_climber_flags_an_eta_past_the_hour(float peakMedian, bool over)
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithSegments(Biome.BiomeType.Shore, Biome.BiomeType.Peak)
            .AfterSeconds(50f)
            .WithBiomeSplits(new BiomeSplit(Biome.BiomeType.Shore, 50f, IsCurrent: true))
            .WithBiomeMedians(new Dictionary<Biome.BiomeType, float> { [Biome.BiomeType.Shore] = 100f, [Biome.BiomeType.Peak] = peakMedian })
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.SpeedClimberBadge).Detail(facts);

        // then
        Assert.Equal(over, Assert.IsType<BadgeDetail.RunClock>(detail).EtaOverLimit);
    }

    [Fact]
    public void Foraging_tells_apart_the_berries_this_map_does_not_spawn()
    {
        // given
        RunFacts facts = new RunFactsBuilder()
            .WithCandidates(RunCollection.DifferentBerriesEaten, 3, 7, 12)
            .WithEaten(RunCollection.DifferentBerriesEaten, 12)
            .WithItemsOnMap(3)
            .Build();

        // when
        BadgeDetail? detail = BadgeRules.For(ACHIEVEMENTTYPE.ForagingBadge).Detail(facts);

        // then: 12 was eaten, so it was on the map whatever the scan saw
        Assert.Equal(
            new BadgeDetail.Checklist(
                [new ChecklistItem(3, Eaten: false, OnMap: true), new ChecklistItem(7, Eaten: false, OnMap: false), new ChecklistItem(12, Eaten: true, OnMap: true)]
            ),
            detail
        );
    }
}

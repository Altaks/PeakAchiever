using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>How the tracker judges one badge: its counter, what can rule it out, and whether it is a clean run.</summary>
internal sealed class BadgeRule
{
    /// <summary>The game only validates the condition at the summit, so the badge shows as held until then.</summary>
    private readonly bool _isCleanRun;
    private readonly IReadOnlyList<IBlocker> _blockers;
    private readonly IDetailSource? _detailSource;

    private BadgeRule(
        IProgressMeasure? progressMeasure,
        bool isCleanRun,
        IReadOnlyList<IBlocker> blockers,
        ItemTraits forbiddenItems = ItemTraits.None,
        IDetailSource? detailSource = null
    )
    {
        ProgressMeasure = progressMeasure;
        _isCleanRun = isCleanRun;
        _blockers = blockers;
        ForbiddenItems = forbiddenItems;
        _detailSource = detailSource;
    }

    public IProgressMeasure? ProgressMeasure { get; }

    public bool IsCleanRun => _isCleanRun;

    /// <summary>Items whose use breaks this badge's clean-run condition.</summary>
    public ItemTraits ForbiddenItems { get; }

    /// <summary>A badge unlocked by reaching a counter's target.</summary>
    public static BadgeRule Counted(IProgressMeasure progressMeasure, params IBlocker[] blockers) =>
        new(progressMeasure, isCleanRun: false, blockers);

    /// <summary>A badge the game grants at the summit if nothing broke the condition along the way.</summary>
    public static BadgeRule CleanRun(params IBlocker[] blockers) => new(null, isCleanRun: true, blockers);

    /// <summary>A clean run whose limit the tracker can measure while it holds.</summary>
    public static BadgeRule CleanRun(IProgressMeasure progressMeasure, params IBlocker[] blockers) =>
        new(progressMeasure, isCleanRun: true, blockers);

    /// <summary>A clean run broken by using a kind of item, marked on the inventory while it holds.</summary>
    public static BadgeRule CleanRunForbidding(ItemTraits forbiddenItems, params IBlocker[] blockers) =>
        new(null, isCleanRun: true, blockers, forbiddenItems);

    /// <summary>A badge unlocked by a single in-game event, with no counter to show.</summary>
    public static BadgeRule OneOff(params IBlocker[] blockers) => new(null, isCleanRun: false, blockers);

    /// <summary>The same rule, with more to show on its card whatever its status.</summary>
    public BadgeRule WithDetail(IDetailSource detailSource) =>
        new(ProgressMeasure, _isCleanRun, _blockers, ForbiddenItems, detailSource);

    /// <summary>For each biome requirement of the badge, the biomes that would meet it.</summary>
    public IEnumerable<IReadOnlyCollection<Biome.BiomeType>> BiomeRequirements =>
        _blockers.OfType<IBiomeRequirement>().Select(requirement => requirement.AnyOf);

    public BadgeDetail? Detail(RunFacts facts) => _detailSource?.Describe(facts);

    public TrackedStatus Evaluate(RunFacts facts, bool isUnlocked)
    {
        Progress? progress = ProgressMeasure?.Measure(facts);
        if (isUnlocked)
            return new TrackedStatus.Achieved(progress);
        foreach (IBlocker blocker in _blockers)
        {
            UnattainableReason? reason = blocker.FindBlock(facts);
            if (reason is not null)
                return new TrackedStatus.Unattainable(reason);
        }
        // After the blockers, so a badge broken earlier keeps its own reason.
        if (facts.RunLost)
            return new TrackedStatus.Unattainable(new UnattainableReason.ConditionBroken(BrokenCondition.RunLost));
        return _isCleanRun ? new TrackedStatus.Holding(progress) : new TrackedStatus.Attainable(progress);
    }
}

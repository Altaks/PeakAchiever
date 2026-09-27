using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>How the tracker judges one badge: its counter, what can rule it out, and whether it is a clean run.</summary>
internal sealed class BadgeRule
{
    /// <summary>The game only validates the condition at the summit, so the badge shows as held until then.</summary>
    private readonly bool _isCleanRun;
    private readonly IReadOnlyList<IBlocker> _blockers;

    private BadgeRule(
        IProgressMeasure? progressMeasure,
        bool isCleanRun,
        IReadOnlyList<IBlocker> blockers,
        ItemTraits forbiddenItems = ItemTraits.None
    )
    {
        ProgressMeasure = progressMeasure;
        _isCleanRun = isCleanRun;
        _blockers = blockers;
        ForbiddenItems = forbiddenItems;
    }

    public IProgressMeasure? ProgressMeasure { get; }

    /// <summary>Items whose use breaks this badge's clean-run condition.</summary>
    public ItemTraits ForbiddenItems { get; }

    /// <summary>A badge unlocked by reaching a counter's target.</summary>
    public static BadgeRule Counted(IProgressMeasure progressMeasure, params IBlocker[] blockers) =>
        new(progressMeasure, isCleanRun: false, blockers);

    /// <summary>A badge the game grants at the summit if nothing broke the condition along the way.</summary>
    public static BadgeRule CleanRun(params IBlocker[] blockers) => new(null, isCleanRun: true, blockers);

    /// <summary>A clean run broken by using a kind of item, marked on the inventory while it holds.</summary>
    public static BadgeRule CleanRunForbidding(ItemTraits forbiddenItems, params IBlocker[] blockers) =>
        new(null, isCleanRun: true, blockers, forbiddenItems);

    /// <summary>A badge unlocked by a single in-game event, with no counter to show.</summary>
    public static BadgeRule OneOff(params IBlocker[] blockers) => new(null, isCleanRun: false, blockers);

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
        return _isCleanRun ? new TrackedStatus.Holding() : new TrackedStatus.Attainable(progress);
    }
}

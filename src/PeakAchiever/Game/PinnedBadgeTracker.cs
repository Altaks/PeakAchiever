using System.Collections.Generic;
using PeakAchiever.Pinning;
using PeakAchiever.Tracking;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>Judges every pinned badge against the current run; the HUD and the inventory marks read the result.</summary>
internal sealed class PinnedBadgeTracker(PinBoard board)
{
    private readonly List<TrackedBadge> _tracked = [];

    /// <summary>The pinned badges in pin order, as of the last <see cref="Evaluate"/>; empty outside a run.</summary>
    public IReadOnlyList<TrackedBadge> Tracked => _tracked;

    public ItemTraits ForbiddenItems { get; private set; }

    /// <param name="facts">The run as of now; null outside a run.</param>
    public void Evaluate(RunFacts? facts)
    {
        _tracked.Clear();
        if (facts != null)
        {
            AchievementManager achievements = Singleton<AchievementManager>.Instance;
            foreach (ACHIEVEMENTTYPE badge in board.Pins)
            {
                BadgeRule rule = BadgeRules.For(badge);
                TrackedStatus status = rule.Evaluate(facts, achievements.IsAchievementUnlocked(badge));
                _tracked.Add(new TrackedBadge(badge, rule, status, rule.Detail(facts), board.IsForAlly(badge)));
            }
        }
        ForbiddenItems = ItemRestrictions.ForbiddenBy(_tracked);
    }
}

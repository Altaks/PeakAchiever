using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>
/// Tells which cards fold to one line: a badge earned this run shows its check mark at full size for a few
/// seconds, then folds; one already earned when first seen shows folded at once. A badge pinned for an ally
/// or for the team stays whole, since the run still matters to the others.
/// </summary>
internal sealed class FoldedCards
{
    public const float FoldDelaySeconds = 5f;

    // When each badge was first seen earned, while it stays pinned and earned.
    private readonly Dictionary<ACHIEVEMENTTYPE, float> _earnedAt = [];
    private readonly HashSet<ACHIEVEMENTTYPE> _seen = [];

    /// <param name="tracked">The pinned badges this refresh; empty outside a run.</param>
    /// <param name="now">An unscaled clock, in seconds.</param>
    /// <returns>The badges whose card shows folded.</returns>
    public IReadOnlyCollection<ACHIEVEMENTTYPE> Update(IReadOnlyList<TrackedBadge> tracked, float now)
    {
        var folded = new HashSet<ACHIEVEMENTTYPE>();
        foreach (TrackedBadge badge in tracked)
        {
            bool firstSeen = _seen.Add(badge.Badge);
            if (badge.Status is not TrackedStatus.Achieved || badge.ForAlly || badge.ForTeam)
            {
                _earnedAt.Remove(badge.Badge);
                continue;
            }
            if (!_earnedAt.ContainsKey(badge.Badge))
                _earnedAt[badge.Badge] = firstSeen ? now - FoldDelaySeconds : now;
            if (now - _earnedAt[badge.Badge] >= FoldDelaySeconds)
                folded.Add(badge.Badge);
        }
        _seen.IntersectWith(tracked.Select(badge => badge.Badge));
        return folded;
    }
}

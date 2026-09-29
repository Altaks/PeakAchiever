using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>
/// Tells which cards turn impossible while the player follows them, so only those tear on screen: a card
/// already impossible when first seen (after a rejoin, or a pin added late) shows torn without tearing.
/// </summary>
internal sealed class TornCards
{
    private readonly Dictionary<ACHIEVEMENTTYPE, bool> _wasImpossible = [];

    /// <param name="tracked">The pinned badges this refresh, in pin order; empty outside a run.</param>
    /// <returns>The badges that just turned impossible, in pin order.</returns>
    public IReadOnlyList<ACHIEVEMENTTYPE> Update(IReadOnlyList<TrackedBadge> tracked)
    {
        var tearing = tracked
            .Where(badge => badge.Status is TrackedStatus.Unattainable && _wasImpossible.TryGetValue(badge.Badge, out bool was) && !was)
            .Select(badge => badge.Badge)
            .ToArray();
        _wasImpossible.Clear();
        foreach (TrackedBadge badge in tracked)
            _wasImpossible[badge.Badge] = badge.Status is TrackedStatus.Unattainable;
        return tearing;
    }

    /// <summary>The cards in column order: whole ones in pin order, then torn ones in the order they tore.</summary>
    public static IReadOnlyList<TrackedBadge> Arrange(IReadOnlyList<TrackedBadge> tracked, IReadOnlyList<ACHIEVEMENTTYPE> tornOrder) =>
        tracked
            .Where(badge => !tornOrder.Contains(badge.Badge))
            .Concat(tornOrder.SelectMany(torn => tracked.Where(badge => badge.Badge == torn)))
            .ToArray();
}

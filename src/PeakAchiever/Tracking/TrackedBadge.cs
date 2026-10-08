using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>A pinned badge with the rule it is judged by, its status and its detail this refresh.</summary>
/// <param name="ForAlly">Pinned while already earned, to help an ally earn it.</param>
/// <param name="ForTeam">Pinned by the host for the whole team.</param>
/// <param name="MissingFor">For a team pin, the scouts with the mod who still miss it.</param>
internal readonly record struct TrackedBadge(
    ACHIEVEMENTTYPE Badge,
    BadgeRule Rule,
    TrackedStatus Status,
    BadgeDetail? Detail,
    bool ForAlly = false,
    bool ForTeam = false,
    IReadOnlyList<string>? MissingFor = null
);

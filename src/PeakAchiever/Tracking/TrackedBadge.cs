namespace PeakAchiever.Tracking;

/// <summary>A pinned badge with the rule it is judged by, its status and its detail this refresh.</summary>
internal readonly record struct TrackedBadge(ACHIEVEMENTTYPE Badge, BadgeRule Rule, TrackedStatus Status, BadgeDetail? Detail);

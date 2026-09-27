namespace PeakAchiever.Tracking;

/// <summary>A pinned badge with the rule it is judged by and its status this refresh.</summary>
internal readonly record struct TrackedBadge(ACHIEVEMENTTYPE Badge, BadgeRule Rule, TrackedStatus Status);

namespace PeakAchiever.Tracking;

internal enum ProgressScope
{
    ThisRun,
    Lifetime,
}

internal readonly record struct Progress(int Current, int Target, ProgressScope Scope)
{
    public float Fraction => Target <= 0 ? 1f : System.Math.Min(1f, (float)Current / Target);
}

/// <summary>What a pinned badge shows on the tracker.</summary>
internal abstract record TrackedStatus
{
    private TrackedStatus() { }

    /// <summary>Still doable this run; <see cref="Progress"/> is null when the game keeps no counter.</summary>
    public sealed record Attainable(Progress? Progress) : TrackedStatus;

    /// <summary>A clean-run condition, intact so far and only validated by the game at the summit.</summary>
    public sealed record Holding : TrackedStatus;

    public sealed record Achieved(Progress? Progress) : TrackedStatus;

    public sealed record Unattainable(UnattainableReason Reason) : TrackedStatus;
}

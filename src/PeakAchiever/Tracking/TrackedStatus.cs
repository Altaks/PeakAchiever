namespace PeakAchiever.Tracking;

internal enum ProgressScope
{
    ThisRun,
    Lifetime,
}

/// <summary>What <see cref="Progress.Current"/> and <see cref="Progress.Target"/> count.</summary>
internal enum ProgressUnit
{
    Count,
    /// <summary>Whole seconds, shown as a clock.</summary>
    Duration,
    /// <summary>Whole percent of the highest rate reached, against the most the game allows.</summary>
    Percent,
}

internal readonly record struct Progress(int Current, int Target, ProgressScope Scope, ProgressUnit Unit = ProgressUnit.Count)
{
    public float Fraction => Target <= 0 ? 1f : System.Math.Min(1f, (float)Current / Target);
}

/// <summary>What a pinned badge shows on the tracker.</summary>
internal abstract record TrackedStatus
{
    private TrackedStatus() { }

    /// <summary>Still doable this run; <see cref="Progress"/> is null when the game keeps no counter.</summary>
    public sealed record Attainable(Progress? Progress) : TrackedStatus;

    /// <summary>
    /// A clean-run condition, intact so far and only validated by the game at the summit;
    /// <see cref="Progress"/> is how close it is to its limit, when it has one.
    /// </summary>
    public sealed record Holding(Progress? Progress = null) : TrackedStatus;

    public sealed record Achieved(Progress? Progress) : TrackedStatus;

    public sealed record Unattainable(UnattainableReason Reason) : TrackedStatus;
}

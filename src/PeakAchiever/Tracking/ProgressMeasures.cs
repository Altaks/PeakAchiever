namespace PeakAchiever.Tracking;

/// <summary>Reads how far a badge's counter has come; the target mirrors the game's own threshold.</summary>
internal interface IProgressMeasure
{
    Progress Measure(RunFacts facts);
}

internal sealed class RunValueTarget(RUNBASEDVALUETYPE value, int target) : IProgressMeasure
{
    public Progress Measure(RunFacts facts) =>
        new((int)facts.RunValue(value), target, ProgressScope.ThisRun);
}

internal sealed class RunCollectionTarget(RunCollection collection, int target) : IProgressMeasure
{
    public Progress Measure(RunFacts facts) =>
        new(facts.CollectionCount(collection), target, ProgressScope.ThisRun);
}

/// <summary>The belltowers lit this run, by anyone, against the count the game grants Bellringer at.</summary>
internal sealed class LitBellsTarget(int target) : IProgressMeasure
{
    public Progress Measure(RunFacts facts) => new(facts.LitBells, target, ProgressScope.ThisRun);
}

/// <summary>The run's elapsed time against the most a clean run may take.</summary>
internal sealed class RunDurationTarget(float maxSeconds) : IProgressMeasure
{
    public Progress Measure(RunFacts facts) =>
        new((int)facts.SecondsSinceRunStarted, (int)maxSeconds, ProgressScope.ThisRun, ProgressUnit.Duration);
}

/// <summary>
/// A run value the game keeps as a fraction of one affliction of the status bar, against the ceiling it checks.
/// </summary>
internal sealed class RunRateTarget(RUNBASEDVALUETYPE value, float ceiling, CharacterAfflictions.STATUSTYPE affliction)
    : IProgressMeasure
{
    private const float PercentPerFraction = 100f;

    public Progress Measure(RunFacts facts) =>
        new(ToPercent(facts.RunValue(value)), ToPercent(ceiling), ProgressScope.ThisRun, ProgressUnit.Percent, affliction);

    // Rounded, not truncated: 0.1f times 100 lands a hair off 10 in float.
    private static int ToPercent(float fraction) => (int)System.Math.Round(fraction * PercentPerFraction);
}

internal sealed class LifetimeStatTarget(STEAMSTATTYPE stat, int target) : IProgressMeasure
{
    public STEAMSTATTYPE Stat => stat;

    public Progress Measure(RunFacts facts) =>
        new(facts.LifetimeStat(stat), target, ProgressScope.Lifetime);
}

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

/// <summary>The run's elapsed time against the most a clean run may take.</summary>
internal sealed class RunDurationTarget(float maxSeconds) : IProgressMeasure
{
    public Progress Measure(RunFacts facts) =>
        new((int)facts.SecondsSinceRunStarted, (int)maxSeconds, ProgressScope.ThisRun, ProgressUnit.Duration);
}

internal sealed class LifetimeStatTarget(STEAMSTATTYPE stat, int target) : IProgressMeasure
{
    public STEAMSTATTYPE Stat => stat;

    public Progress Measure(RunFacts facts) =>
        new(facts.LifetimeStat(stat), target, ProgressScope.Lifetime);
}

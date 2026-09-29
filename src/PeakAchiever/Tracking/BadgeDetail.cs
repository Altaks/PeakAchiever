using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>What a card shows below its status, whatever that status is.</summary>
internal abstract record BadgeDetail
{
    private BadgeDetail() { }

    /// <summary>How long the run has lasted, and how that time splits across the biomes climbed.</summary>
    public sealed record RunClock(float ElapsedSeconds, IReadOnlyList<BiomeSplit> Splits) : BadgeDetail
    {
        public bool Equals(RunClock? other) =>
            other is not null && ElapsedSeconds.Equals(other.ElapsedSeconds) && Splits.SequenceEqual(other.Splits);

        public override int GetHashCode() => ElapsedSeconds.GetHashCode();
    }
}

/// <summary>Reads a card's detail from the run.</summary>
internal interface IDetailSource
{
    BadgeDetail Describe(RunFacts facts);
}

internal sealed class RunClockSource : IDetailSource
{
    public BadgeDetail Describe(RunFacts facts) => new BadgeDetail.RunClock(facts.SecondsSinceRunStarted, facts.BiomeSplits);
}

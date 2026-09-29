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

    /// <summary>Every item that counts towards the badge, in the game's id order, ticked once eaten.</summary>
    public sealed record Checklist(IReadOnlyList<ChecklistItem> Items) : BadgeDetail
    {
        public bool Equals(Checklist? other) => other is not null && Items.SequenceEqual(other.Items);

        public override int GetHashCode() => Items.Count;
    }
}

internal readonly record struct ChecklistItem(ushort ItemId, bool Eaten);

/// <summary>Reads a card's detail from the run.</summary>
internal interface IDetailSource
{
    BadgeDetail Describe(RunFacts facts);
}

internal sealed class EatenItemsSource(RunCollection collection) : IDetailSource
{
    private static readonly IReadOnlyCollection<ushort> NothingEaten = [];

    public BadgeDetail Describe(RunFacts facts)
    {
        IReadOnlyCollection<ushort> eaten = facts.EatenItems.TryGetValue(collection, out IReadOnlyCollection<ushort> ids)
            ? ids
            : NothingEaten;
        IReadOnlyList<ushort> candidates = facts.CollectionCandidates.TryGetValue(collection, out IReadOnlyList<ushort> all)
            ? all
            : [];
        return new BadgeDetail.Checklist(candidates.Select(item => new ChecklistItem(item, eaten.Contains(item))).ToArray());
    }
}

internal sealed class RunClockSource : IDetailSource
{
    public BadgeDetail Describe(RunFacts facts) => new BadgeDetail.RunClock(facts.SecondsSinceRunStarted, facts.BiomeSplits);
}

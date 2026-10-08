using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>What a card shows below its status, whatever that status is.</summary>
internal abstract record BadgeDetail
{
    private BadgeDetail() { }

    /// <summary>
    /// How long the run has lasted, how that time splits across the biomes climbed, and when past runs
    /// say the summit should be reached (null until they cover every biome left), flagged when that is
    /// past the time limit.
    /// </summary>
    public sealed record RunClock(float ElapsedSeconds, IReadOnlyList<ComparedSplit> Splits, float? EtaSeconds, bool EtaOverLimit)
        : BadgeDetail
    {
        public bool Equals(RunClock? other) =>
            other is not null
            && ElapsedSeconds.Equals(other.ElapsedSeconds)
            && Splits.SequenceEqual(other.Splits)
            && EtaSeconds.Equals(other.EtaSeconds)
            && EtaOverLimit == other.EtaOverLimit;

        public override int GetHashCode() => ElapsedSeconds.GetHashCode();
    }

    /// <summary>
    /// Every item that counts towards the badge, in the game's id order, ticked once eaten, and told apart
    /// when this map yields none.
    /// </summary>
    public sealed record Checklist(IReadOnlyList<ChecklistItem> Items) : BadgeDetail
    {
        public bool Equals(Checklist? other) => other is not null && Items.SequenceEqual(other.Items);

        public override int GetHashCode() => Items.Count;
    }

    /// <summary>
    /// The nearest biome ahead where the badge can be earned, and how many biome stretches away it is
    /// (0 while the team is in it).
    /// </summary>
    public sealed record BiomeAhead(Biome.BiomeType Biome, int StretchesAhead) : BadgeDetail;
}

/// <param name="OnMap">False only when the map was read and nothing on it yields the item: a hint, not proof.</param>
internal readonly record struct ChecklistItem(ushort ItemId, bool Eaten, bool OnMap);

/// <summary>Reads a card's detail from the run.</summary>
internal interface IDetailSource
{
    /// <returns>Null when there is nothing to show this refresh.</returns>
    BadgeDetail? Describe(RunFacts facts);
}

internal sealed class EatenItemsSource(RunCollection collection) : IDetailSource
{
    private static readonly IReadOnlyCollection<ushort> NothingEaten = [];

    public BadgeDetail? Describe(RunFacts facts)
    {
        IReadOnlyCollection<ushort> eaten = facts.EatenItems.TryGetValue(collection, out IReadOnlyCollection<ushort> ids)
            ? ids
            : NothingEaten;
        IReadOnlyList<ushort> candidates = facts.CollectionCandidates.TryGetValue(collection, out IReadOnlyList<ushort> all)
            ? all
            : [];
        return new BadgeDetail.Checklist(
            candidates
                .Select(item =>
                {
                    bool wasEaten = eaten.Contains(item);
                    // Eaten means it was here, whatever the scan of the map missed.
                    bool onMap = wasEaten || facts.ItemsOnMap is null || facts.ItemsOnMap.Contains(item);
                    return new ChecklistItem(item, wasEaten, onMap);
                })
                .ToArray()
        );
    }
}

internal sealed class RunClockSource(float limitSeconds) : IDetailSource
{
    public BadgeDetail? Describe(RunFacts facts)
    {
        float? eta = RunEta.Estimate(facts);
        return new BadgeDetail.RunClock(
            facts.SecondsSinceRunStarted,
            facts.BiomeSplits.Select(split => ComparedSplit.Against(split, facts.BiomeMedians)).ToArray(),
            eta,
            EtaOverLimit: eta > limitSeconds
        );
    }
}

/// <summary>
/// Where the badge's biome is from here: the first segment ahead holding one of <paramref name="biomes"/>.
/// Nothing once they are all behind (the red cross tells it then), or in the Nadir.
/// </summary>
internal sealed class BiomeAheadSource(params Biome.BiomeType[] biomes) : IDetailSource
{
    public BadgeDetail? Describe(RunFacts facts)
    {
        if (facts.CurrentSegmentIndex is not { } current)
            return null;
        IReadOnlyList<Biome.BiomeType> segments = facts.SegmentBiomes;
        int stretches = 0;
        for (int segment = current; segment < segments.Count; segment++)
        {
            // Consecutive segments of one biome make a single stretch (BiomeTimeline.Split).
            if (segment > current && segments[segment] != segments[segment - 1])
                stretches++;
            if (biomes.Contains(segments[segment]))
                return new BadgeDetail.BiomeAhead(segments[segment], stretches);
        }
        return null;
    }
}

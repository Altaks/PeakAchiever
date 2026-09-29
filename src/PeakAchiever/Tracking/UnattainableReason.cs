using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>Why a badge can no longer be earned this run, shown under the red cross.</summary>
internal abstract record UnattainableReason
{
    private UnattainableReason() { }

    /// <summary>None of the biomes where the badge can be earned is on this map.</summary>
    public sealed record BiomeAbsent(params Biome.BiomeType[] Biomes) : UnattainableReason
    {
        public bool Equals(BiomeAbsent? other) => other is not null && Biomes.SequenceEqual(other.Biomes);

        public override int GetHashCode() => Biomes.Length;
    }

    /// <summary>Every biome where the badge can be earned is behind the team.</summary>
    public sealed record BiomeLeft(params Biome.BiomeType[] Biomes) : UnattainableReason
    {
        public bool Equals(BiomeLeft? other) => other is not null && Biomes.SequenceEqual(other.Biomes);

        public override int GetHashCode() => Biomes.Length;
    }

    public sealed record ConditionBroken(BrokenCondition Condition) : UnattainableReason;
}

/// <summary>The clean-run conditions a player can break, each with its own explanation text.</summary>
internal enum BrokenCondition
{
    TookFallDamage,
    AtePackagedFood,
    PassedOut,
    PlacedPermanentItem,
    RunTooLong,
    TooMuchHeat,
    TooMuchCold,
    TooManySpores,
    HitByTrap,
    /// <summary>The run ended with nobody at the summit: nothing more can be earned in it.</summary>
    RunLost,
}

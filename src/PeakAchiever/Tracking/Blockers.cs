using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>One way a badge becomes impossible for the rest of the run.</summary>
internal interface IBlocker
{
    /// <returns>The reason the badge is out of reach, or null while it still is not.</returns>
    UnattainableReason? FindBlock(RunFacts facts);
}

/// <summary>
/// The badge needs this biome on the map, but the game may grant it after the biome is left
/// (area badges, and clean-biome checks run when the next area is reached).
/// </summary>
internal sealed class BiomeOnMap(Biome.BiomeType biome) : IBlocker
{
    public UnattainableReason? FindBlock(RunFacts facts) =>
        facts.PresentBiomes.Contains(biome) ? null : new UnattainableReason.BiomeAbsent(biome);
}

/// <summary>The badge can only be earned while inside one of these biomes.</summary>
internal sealed class InBiome(params Biome.BiomeType[] biomes) : IBlocker
{
    public UnattainableReason? FindBlock(RunFacts facts)
    {
        Biome.BiomeType[] onMap = biomes.Where(facts.PresentBiomes.Contains).ToArray();
        if (onMap.Length == 0)
            return new UnattainableReason.BiomeAbsent(biomes);
        if (onMap.All(facts.HasLeft))
            return new UnattainableReason.BiomeLeft(onMap);
        return null;
    }
}

/// <summary>Broken as soon as a run counter goes above the ceiling the game checks.</summary>
internal sealed class RunValueCeiling(RUNBASEDVALUETYPE value, float ceiling, BrokenCondition condition)
    : IBlocker
{
    public UnattainableReason? FindBlock(RunFacts facts) =>
        facts.RunValue(value) > ceiling ? new UnattainableReason.ConditionBroken(condition) : null;
}

internal sealed class RunDurationCeiling(float maxSeconds) : IBlocker
{
    // AchievementManager.TestTimeAchievements floors the run time before comparing it (v2.4.c).
    public UnattainableReason? FindBlock(RunFacts facts) =>
        System.Math.Floor(facts.SecondsSinceRunStarted) > maxSeconds
            ? new UnattainableReason.ConditionBroken(BrokenCondition.RunTooLong)
            : null;
}

internal sealed class SoloOnly : IBlocker
{
    public UnattainableReason? FindBlock(RunFacts facts) =>
        facts.ScoutCount > 1 ? new UnattainableReason.ConditionBroken(BrokenCondition.NotSolo) : null;
}

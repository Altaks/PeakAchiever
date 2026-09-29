using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>
/// Which badges can be earned in the same run, going by the biomes of every map the game can pick.
/// Only a proven clash counts: a biome the table never lists, or an empty table, rules nothing out.
/// </summary>
internal sealed class BadgeCompatibility
{
    public static readonly BadgeCompatibility Unconstrained = new([]);

    private readonly IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> _maps;
    private readonly HashSet<Biome.BiomeType> _listedBiomes;

    /// <param name="maps">The biomes of each map the game can pick.</param>
    public BadgeCompatibility(IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> maps)
    {
        _maps = maps;
        _listedBiomes = maps.SelectMany(map => map).ToHashSet();
    }

    public bool CanShareARun(ACHIEVEMENTTYPE first, ACHIEVEMENTTYPE second)
    {
        if (_maps.Count == 0)
            return true;
        IReadOnlyCollection<Biome.BiomeType>[] requirements = BadgeRules
            .For(first)
            .BiomeRequirements.Concat(BadgeRules.For(second).BiomeRequirements)
            .Where(anyOf => anyOf.All(_listedBiomes.Contains))
            .ToArray();
        return _maps.Any(map => requirements.All(anyOf => anyOf.Any(map.Contains)));
    }

    /// <returns>The first of <paramref name="pinned"/> that cannot share a run with <paramref name="badge"/>, if any.</returns>
    public ACHIEVEMENTTYPE? FirstConflict(ACHIEVEMENTTYPE badge, IEnumerable<ACHIEVEMENTTYPE> pinned)
    {
        foreach (ACHIEVEMENTTYPE other in pinned)
        {
            if (!CanShareARun(badge, other))
                return other;
        }
        return null;
    }
}

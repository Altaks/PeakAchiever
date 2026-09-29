using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>
/// Which badges a map allows, and which can be earned in the same run, going by the biomes of every map
/// the game can pick. Only a proven clash counts: a biome the table never lists, or an empty table,
/// rules nothing out.
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
        IReadOnlyCollection<Biome.BiomeType>[] requirements = ProvenRequirements(first).Concat(ProvenRequirements(second)).ToArray();
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

    /// <returns>The biomes of the first requirement of <paramref name="badge"/> this map cannot meet; empty when it meets them all.</returns>
    public IReadOnlyCollection<Biome.BiomeType> MissingOn(ACHIEVEMENTTYPE badge, IReadOnlyCollection<Biome.BiomeType> map) =>
        ProvenRequirements(badge).FirstOrDefault(anyOf => !anyOf.Any(map.Contains)) ?? [];

    // A requirement naming a biome the table never lists could be met by a map it does not know of.
    private IEnumerable<IReadOnlyCollection<Biome.BiomeType>> ProvenRequirements(ACHIEVEMENTTYPE badge) =>
        BadgeRules.For(badge).BiomeRequirements.Where(anyOf => anyOf.All(_listedBiomes.Contains));
}

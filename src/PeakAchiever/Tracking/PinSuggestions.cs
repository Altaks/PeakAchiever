using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Tracking;

/// <summary>Which badges to pin for a map, to fill the free slots with a set that can all be earned together.</summary>
internal static class PinSuggestions
{
    /// <param name="unearned">The badges to choose from, in the order to show them in (the game's).</param>
    /// <param name="map">The biomes of the map the run is, or will be, on.</param>
    /// <returns>
    /// Up to <paramref name="freeSlots"/> badges the map allows, compatible with the pins and with each other:
    /// first those tied to this map's biomes (the map alternates, so they wait for another day otherwise),
    /// then clean runs, then the rest.
    /// </returns>
    public static IReadOnlyList<ACHIEVEMENTTYPE> For(
        IEnumerable<ACHIEVEMENTTYPE> unearned,
        IReadOnlyCollection<ACHIEVEMENTTYPE> pinned,
        int freeSlots,
        IReadOnlyCollection<Biome.BiomeType> map,
        BadgeCompatibility compatibility
    )
    {
        var chosen = new List<ACHIEVEMENTTYPE>();
        IEnumerable<ACHIEVEMENTTYPE> ranked = unearned
            .Where(badge => !pinned.Contains(badge) && compatibility.MissingOn(badge, map).Count == 0)
            .OrderBy(Rank);
        foreach (ACHIEVEMENTTYPE badge in ranked)
        {
            if (chosen.Count >= freeSlots)
                break;
            if (compatibility.FirstConflict(badge, pinned.Concat(chosen)) is null)
                chosen.Add(badge);
        }
        return chosen;
    }

    // OrderBy is stable, so badges of one rank keep the game's order.
    private static int Rank(ACHIEVEMENTTYPE badge)
    {
        BadgeRule rule = BadgeRules.For(badge);
        if (rule.BiomeRequirements.Any())
            return 0;
        return rule.IsCleanRun ? 1 : 2;
    }
}

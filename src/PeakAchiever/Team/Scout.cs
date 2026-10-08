using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Team;

/// <summary>A player in the game, and the badges they have earned; null when they do not run the mod.</summary>
internal sealed record Scout(string Name, bool IsLocal, IReadOnlyCollection<ACHIEVEMENTTYPE>? Earned);

internal static class Scouts
{
    /// <summary>
    /// True when every scout whose badges are known has earned it. With nobody known it proves nothing,
    /// so it is false: a scout without the mod may still need it.
    /// </summary>
    public static bool EarnedByAll(ACHIEVEMENTTYPE badge, IEnumerable<Scout> scouts)
    {
        Scout[] known = scouts.Where(scout => scout.Earned != null).ToArray();
        return known.Length > 0 && known.All(scout => scout.Earned!.Contains(badge));
    }

    /// <summary>The scouts whose badges are known and who have not earned it; scouts without the mod are left out.</summary>
    public static IReadOnlyList<string> MissingFor(ACHIEVEMENTTYPE badge, IEnumerable<Scout> scouts) =>
        scouts.Where(scout => scout.Earned != null && !scout.Earned.Contains(badge)).Select(scout => scout.Name).ToArray();
}

using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Team;

/// <summary>A badge the host can pin for the team: how many known scouts have it, and who still misses it.</summary>
internal sealed record TeamRow(ACHIEVEMENTTYPE Badge, int EarnedBy, int Known, IReadOnlyList<string> MissingFor)
{
    /// <summary>No scout whose badges are known has it: the recommended kind.</summary>
    public bool NobodyHasIt => EarnedBy == 0;
}

internal static class TeamRanking
{
    /// <summary>
    /// The badges worth pinning for the team: first those nobody has, then the most missing; a badge every
    /// known scout already has is left out. Ties keep <paramref name="candidates"/>' order (the game's).
    /// Scouts without the mod count in neither side.
    /// </summary>
    public static IReadOnlyList<TeamRow> Rank(IEnumerable<ACHIEVEMENTTYPE> candidates, IReadOnlyList<Scout> scouts)
    {
        Scout[] known = scouts.Where(scout => scout.Earned != null).ToArray();
        return candidates
            .Select(badge =>
            {
                string[] missing = known.Where(scout => !scout.Earned!.Contains(badge)).Select(scout => scout.Name).ToArray();
                return new TeamRow(badge, known.Length - missing.Length, known.Length, missing);
            })
            .Where(row => row.Known == 0 || row.MissingFor.Count > 0)
            // A badge nobody has is missing for every known scout, the most there is: it sorts first.
            .OrderByDescending(row => row.MissingFor.Count)
            .ToArray();
    }
}

using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Team;

internal static class TrackedPins
{
    /// <summary>
    /// The badges the tracker follows, in column order: the host's team pins first, then the player's own;
    /// a badge in both shows once, as a team pin.
    /// </summary>
    public static IReadOnlyList<(ACHIEVEMENTTYPE Badge, bool ForTeam)> Merge(IReadOnlyList<ACHIEVEMENTTYPE> team, IReadOnlyList<ACHIEVEMENTTYPE> own) =>
        team.Select(badge => (badge, ForTeam: true)).Concat(own.Where(badge => !team.Contains(badge)).Select(badge => (badge, ForTeam: false))).ToArray();
}

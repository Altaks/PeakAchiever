using System;
using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Team;

/// <summary>
/// Badges written as their names, comma-separated: in the config and in the Photon properties players
/// share. Names, not numbers, so a list survives a game update that renumbers the badges.
/// </summary>
internal static class BadgeNames
{
    public const char Separator = ',';

    public sealed record Parsed(IReadOnlyList<ACHIEVEMENTTYPE> Badges, IReadOnlyList<string> Unknown);

    public static string Join(IEnumerable<ACHIEVEMENTTYPE> badges) => string.Join(Separator.ToString(), badges);

    /// <remarks>Names this game does not know (another version's) go to <see cref="Parsed.Unknown"/>.</remarks>
    public static Parsed Parse(string text)
    {
        var badges = new List<ACHIEVEMENTTYPE>();
        var unknown = new List<string>();
        foreach (string name in text.Split([Separator], StringSplitOptions.RemoveEmptyEntries).Select(name => name.Trim()))
        {
            if (!TryParse(name, out ACHIEVEMENTTYPE badge))
                unknown.Add(name);
            else if (!badges.Contains(badge))
                badges.Add(badge);
        }
        return new Parsed(badges, unknown);
    }

    /// <summary>A real badge's name: not a number, not NONE, one this game defines.</summary>
    public static bool TryParse(string name, out ACHIEVEMENTTYPE badge) =>
        Enum.TryParse(name, out badge)
        && !int.TryParse(name, out _)
        && Enum.IsDefined(typeof(ACHIEVEMENTTYPE), badge)
        && badge != ACHIEVEMENTTYPE.NONE;
}

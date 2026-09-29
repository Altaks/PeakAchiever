using System;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Team;

namespace PeakAchiever.Pinning;

/// <summary>
/// The pins as the config keeps them: badge names in pin order, separated by commas, a leading plus on a
/// badge pinned to help an ally ("CookingBadge,+PeakBadge"). A list saved by 0.2.0 has no plus.
/// </summary>
internal sealed record PinList(IReadOnlyList<ACHIEVEMENTTYPE> Pins, IReadOnlyList<ACHIEVEMENTTYPE> ForAllies, IReadOnlyList<string> Unknown)
{
    private const char Separator = BadgeNames.Separator;
    private const char AllyMark = '+';

    public static string Write(PinBoard board) =>
        string.Join(Separator.ToString(), board.Pins.Select(badge => board.IsForAlly(badge) ? AllyMark + badge.ToString() : badge.ToString()));

    /// <remarks>Names it cannot read go to <see cref="Unknown"/>, for the caller to report.</remarks>
    public static PinList Read(string stored)
    {
        var pins = new List<ACHIEVEMENTTYPE>();
        var forAllies = new List<ACHIEVEMENTTYPE>();
        var unknown = new List<string>();
        foreach (string entry in stored.Split([Separator], StringSplitOptions.RemoveEmptyEntries).Select(entry => entry.Trim()))
        {
            bool forAlly = entry.StartsWith(AllyMark.ToString(), StringComparison.Ordinal);
            string name = entry.TrimStart(AllyMark);
            if (!BadgeNames.TryParse(name, out ACHIEVEMENTTYPE badge))
            {
                unknown.Add(name);
                continue;
            }
            if (pins.Contains(badge))
                continue;
            pins.Add(badge);
            if (forAlly)
                forAllies.Add(badge);
        }
        return new PinList(pins, forAllies, unknown);
    }
}

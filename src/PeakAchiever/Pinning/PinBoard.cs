using System;
using System.Collections.Generic;
using PeakAchiever.Tracking;

namespace PeakAchiever.Pinning;

internal enum PinToggleOutcome
{
    Pinned,
    Unpinned,
    RejectedBoardFull,
    RejectedConflict,
}

/// <summary>
/// The badges the player chose to chase, in the order they were pinned. A badge pinned while already
/// earned is pinned to help an ally earn it, and stays pinned from one run to the next.
/// </summary>
internal sealed class PinBoard(IEnumerable<ACHIEVEMENTTYPE> pins, int capacity, IEnumerable<ACHIEVEMENTTYPE>? pinnedForAllies = null)
{
    private readonly List<ACHIEVEMENTTYPE> _pins = [.. pins];
    private readonly HashSet<ACHIEVEMENTTYPE> _forAllies = [.. pinnedForAllies ?? []];

    public IReadOnlyList<ACHIEVEMENTTYPE> Pins => _pins;

    public int Capacity { get; set; } = capacity;

    public bool IsPinned(ACHIEVEMENTTYPE badge) => _pins.Contains(badge);

    /// <summary>True for a badge pinned while already earned, to help an ally.</summary>
    public bool IsForAlly(ACHIEVEMENTTYPE badge) => _forAllies.Contains(badge);

    public PinToggleOutcome Toggle(ACHIEVEMENTTYPE badge, bool isEarned, BadgeCompatibility compatibility)
    {
        if (_pins.Remove(badge))
        {
            _forAllies.Remove(badge);
            return PinToggleOutcome.Unpinned;
        }
        if (compatibility.FirstConflict(badge, _pins) is not null)
            return PinToggleOutcome.RejectedConflict;
        if (_pins.Count >= Capacity)
            return PinToggleOutcome.RejectedBoardFull;
        _pins.Add(badge);
        if (isEarned)
            _forAllies.Add(badge);
        return PinToggleOutcome.Pinned;
    }

    /// <summary>
    /// Clears badges earned in a previous run, since there is nothing left to chase; those pinned to
    /// help an ally stay.
    /// </summary>
    /// <returns>True when at least one pin was removed.</returns>
    public bool DropEarned(Predicate<ACHIEVEMENTTYPE> isEarned) => _pins.RemoveAll(badge => isEarned(badge) && !IsForAlly(badge)) > 0;
}

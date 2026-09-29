using System;
using System.Collections.Generic;
using PeakAchiever.Tracking;

namespace PeakAchiever.Pinning;

internal enum PinToggleOutcome
{
    Pinned,
    Unpinned,
    RejectedBoardFull,
    RejectedAlreadyEarned,
    RejectedConflict,
}

/// <summary>The badges the player chose to chase, in the order they were pinned.</summary>
internal sealed class PinBoard(IEnumerable<ACHIEVEMENTTYPE> pins, int capacity)
{
    private readonly List<ACHIEVEMENTTYPE> _pins = [.. pins];

    public IReadOnlyList<ACHIEVEMENTTYPE> Pins => _pins;

    public int Capacity { get; set; } = capacity;

    public bool IsPinned(ACHIEVEMENTTYPE badge) => _pins.Contains(badge);

    public PinToggleOutcome Toggle(ACHIEVEMENTTYPE badge, bool isEarned, BadgeCompatibility compatibility)
    {
        if (_pins.Remove(badge))
            return PinToggleOutcome.Unpinned;
        if (isEarned)
            return PinToggleOutcome.RejectedAlreadyEarned;
        if (compatibility.FirstConflict(badge, _pins) is not null)
            return PinToggleOutcome.RejectedConflict;
        if (_pins.Count >= Capacity)
            return PinToggleOutcome.RejectedBoardFull;
        _pins.Add(badge);
        return PinToggleOutcome.Pinned;
    }

    /// <summary>Clears badges earned in a previous run, since there is nothing left to chase.</summary>
    /// <returns>True when at least one pin was removed.</returns>
    public bool DropEarned(Predicate<ACHIEVEMENTTYPE> isEarned) => _pins.RemoveAll(isEarned) > 0;
}

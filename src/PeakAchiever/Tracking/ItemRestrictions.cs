using System.Collections.Generic;

namespace PeakAchiever.Tracking;

internal static class ItemRestrictions
{
    /// <summary>
    /// The item traits the player must avoid: those forbidden by pinned clean-run badges still holding.
    /// Once a badge is earned or broken, its items are free again.
    /// </summary>
    public static ItemTraits ForbiddenBy(IEnumerable<TrackedBadge> tracked)
    {
        ItemTraits forbidden = ItemTraits.None;
        foreach (TrackedBadge badge in tracked)
        {
            if (badge.Status is TrackedStatus.Holding)
                forbidden |= badge.Rule.ForbiddenItems;
        }
        return forbidden;
    }
}

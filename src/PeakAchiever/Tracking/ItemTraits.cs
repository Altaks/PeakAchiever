using System;

namespace PeakAchiever.Tracking;

/// <summary>What using an item does that a clean-run badge may forbid.</summary>
[Flags]
internal enum ItemTraits
{
    None = 0,

    /// <summary>Eating it counts as packaged food (AchievementManager.TestItemConsumed).</summary>
    PackagedFood = 1 << 0,

    /// <summary>Using it leaves a permanent object on the mountain (GameUtils.IncrementPermanentItemsPlaced).</summary>
    PlacesPermanentObject = 1 << 1,
}

using System.Collections.Generic;

namespace PeakAchiever.Tracking;

/// <summary>
/// The distinct-item lists the game keeps per run in <c>SerializableRunBasedValues</c>,
/// which have no <see cref="RUNBASEDVALUETYPE"/> of their own.
/// </summary>
internal enum RunCollection
{
    DifferentBerriesEaten,
    DifferentShroomBerriesEaten,
    DifferentNonToxicMushroomsEaten,
    GourmandDishesEaten,
}

internal static class RunCollections
{
    /// <summary>
    /// The lists eating this item adds it to, as AchievementManager.TestItemConsumed sorts it (v2.4.c).
    /// A Gourmand dish only counts once cooked; that is checked on the eaten item, not here.
    /// </summary>
    public static IEnumerable<RunCollection> CountingItem(Item.ItemTags tags, bool poisons)
    {
        bool berry = tags.HasFlag(Item.ItemTags.Berry);
        bool mushroom = tags.HasFlag(Item.ItemTags.Mushroom);
        if (berry)
            yield return RunCollection.DifferentBerriesEaten;
        if (berry && mushroom)
            yield return RunCollection.DifferentShroomBerriesEaten;
        if (mushroom && !berry && !poisons)
            yield return RunCollection.DifferentNonToxicMushroomsEaten;
        if (tags.HasFlag(Item.ItemTags.GourmandRequirement))
            yield return RunCollection.GourmandDishesEaten;
    }
}

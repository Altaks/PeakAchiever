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

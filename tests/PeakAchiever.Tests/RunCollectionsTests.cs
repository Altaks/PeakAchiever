using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class RunCollectionsTests
{
    [Theory]
    [InlineData(Item.ItemTags.Berry, false, new[] { RunCollection.DifferentBerriesEaten })]
    [InlineData(
        Item.ItemTags.Berry | Item.ItemTags.Mushroom,
        false,
        new[] { RunCollection.DifferentBerriesEaten, RunCollection.DifferentShroomBerriesEaten }
    )]
    [InlineData(Item.ItemTags.Mushroom, false, new[] { RunCollection.DifferentNonToxicMushroomsEaten })]
    [InlineData(Item.ItemTags.Mushroom, true, new RunCollection[0])]
    [InlineData(Item.ItemTags.GourmandRequirement, false, new[] { RunCollection.GourmandDishesEaten })]
    [InlineData(Item.ItemTags.PackagedFood, false, new RunCollection[0])]
    // RunCollection is internal, so a public theory takes the expected lists as object.
    public void An_item_counts_towards_the_lists_the_game_adds_it_to(Item.ItemTags tags, bool poisons, object expected)
    {
        // when
        RunCollection[] collections = RunCollections.CountingItem(tags, poisons).ToArray();

        // then
        Assert.Equal((RunCollection[])expected, collections);
    }
}

using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class ItemSourcesTests
{
    [Fact]
    public void Items_the_map_spawns_directly_are_on_it()
    {
        // when
        IReadOnlyCollection<ushort> onMap = ItemSources.Reachable([4, 9], new Dictionary<ushort, IReadOnlyCollection<ushort>>());

        // then
        Assert.Equal(new ushort[] { 4, 9 }, onMap.Order());
    }

    [Fact]
    public void An_item_another_item_turns_into_is_on_the_map_too_however_deep()
    {
        // given: 4 spawns 7 when used, 7 spawns 12
        var spawnsOnUse = new Dictionary<ushort, IReadOnlyCollection<ushort>> { [4] = [7], [7] = [12], [30] = [31] };

        // when
        IReadOnlyCollection<ushort> onMap = ItemSources.Reachable([4], spawnsOnUse);

        // then
        Assert.Equal(new ushort[] { 4, 7, 12 }, onMap.Order());
    }

    [Fact]
    public void A_cycle_of_items_spawning_each_other_ends()
    {
        // given
        var spawnsOnUse = new Dictionary<ushort, IReadOnlyCollection<ushort>> { [1] = [2], [2] = [1] };

        // when
        IReadOnlyCollection<ushort> onMap = ItemSources.Reachable([1], spawnsOnUse);

        // then
        Assert.Equal(new ushort[] { 1, 2 }, onMap.Order());
    }
}

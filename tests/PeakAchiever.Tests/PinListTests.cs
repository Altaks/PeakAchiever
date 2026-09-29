using PeakAchiever.Pinning;

namespace PeakAchiever.Tests;

public class PinListTests
{
    [Fact]
    public void A_pin_for_an_ally_is_written_with_a_plus()
    {
        // given
        var board = new PinBoard(
            [ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.PeakBadge],
            capacity: 5,
            pinnedForAllies: [ACHIEVEMENTTYPE.PeakBadge]
        );

        // when
        string stored = PinList.Write(board);

        // then
        Assert.Equal("CookingBadge,+PeakBadge", stored);
    }

    [Fact]
    public void The_list_reads_back_with_its_order_and_its_ally_pins()
    {
        // when
        PinList read = PinList.Read(" CookingBadge , +PeakBadge,");

        // then
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.PeakBadge], read.Pins);
        Assert.Equal([ACHIEVEMENTTYPE.PeakBadge], read.ForAllies);
        Assert.Empty(read.Unknown);
    }

    [Fact]
    public void A_list_saved_by_0_2_0_reads_without_ally_pins()
    {
        // when
        PinList read = PinList.Read("CookingBadge,PeakBadge");

        // then
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge, ACHIEVEMENTTYPE.PeakBadge], read.Pins);
        Assert.Empty(read.ForAllies);
    }

    [Theory]
    [InlineData("NoSuchBadge")]
    [InlineData("+NoSuchBadge")]
    [InlineData("NONE")]
    public void An_unknown_name_is_set_aside(string stored)
    {
        // when
        PinList read = PinList.Read(stored);

        // then
        Assert.Empty(read.Pins);
        Assert.Equal([stored.TrimStart('+')], read.Unknown);
    }

    [Fact]
    public void A_badge_listed_twice_is_pinned_once()
    {
        // when
        PinList read = PinList.Read("CookingBadge,CookingBadge");

        // then
        Assert.Equal([ACHIEVEMENTTYPE.CookingBadge], read.Pins);
    }
}

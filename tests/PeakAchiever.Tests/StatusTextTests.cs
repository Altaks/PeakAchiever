using PeakAchiever.Hud;
using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class StatusTextTests
{
    [Fact]
    public void A_counter_reads_current_over_target()
    {
        // given
        var progress = new Progress(3, 5, ProgressScope.ThisRun);

        // when
        string text = StatusText.Count(progress);

        // then
        Assert.Equal("3 / 5", text);
    }

    [Fact]
    public void A_duration_reads_as_clocks_like_the_end_screen()
    {
        // given
        var progress = new Progress(2533, 3600, ProgressScope.ThisRun, ProgressUnit.Duration);

        // when
        string text = StatusText.Count(progress);

        // then
        Assert.Equal("0:42:13 / 1:00:00", text);
    }

    [Theory]
    [InlineData(0f, "0:00:00")]
    [InlineData(59.9f, "0:00:59")]
    [InlineData(3891.4f, "1:04:51")]
    public void Clock_floors_to_the_second(float seconds, string expected)
    {
        // when
        string text = StatusText.Clock(seconds);

        // then
        Assert.Equal(expected, text);
    }
}

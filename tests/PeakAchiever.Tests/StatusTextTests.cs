using PeakAchiever.Hud;
using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class StatusTextTests
{
    private static readonly ClockColors AnyColors = new(Current: "FFFF00", Slower: "FF0000", Faster: "00FF00", Caution: "FF9900");

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

    [Fact]
    public void A_rate_reads_as_the_highest_percent_over_the_limit()
    {
        // given
        var progress = new Progress(4, 10, ProgressScope.ThisRun, ProgressUnit.Percent);

        // when
        string text = StatusText.Count(progress);

        // then
        Assert.Equal("max 4% / 10%", text);
    }

    [Fact]
    public void A_broken_run_clock_leads_with_the_elapsed_time_and_the_eta()
    {
        // given
        var clock = new BadgeDetail.RunClock(3891f, [], EtaSeconds: 5880f, EtaOverLimit: false);

        // when
        string text = StatusText.RunClock(clock, withElapsed: true, AnyColors);

        // then
        Assert.Equal("1:04:51 \u00B7 ETA 1:38:00", text);
    }

    [Fact]
    public void A_run_clock_with_no_eta_and_no_split_yet_says_nothing()
    {
        // given
        var clock = new BadgeDetail.RunClock(2f, [], EtaSeconds: null, EtaOverLimit: false);

        // when
        string text = StatusText.RunClock(clock, withElapsed: false, AnyColors);

        // then
        Assert.Equal("", text);
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

    [Theory]
    [InlineData(80f, "+1:20")]
    [InlineData(-40.6f, "-0:40")]
    [InlineData(3733f, "+1:02:13")]
    [InlineData(0f, "+0:00")]
    public void A_delta_reads_signed_with_hours_only_when_needed(float seconds, string expected)
    {
        // when
        string text = StatusText.Delta(seconds);

        // then
        Assert.Equal(expected, text);
    }

    [Fact]
    public void An_eta_past_the_hour_reads_in_the_caution_colour()
    {
        // given
        var clock = new BadgeDetail.RunClock(1200f, [], EtaSeconds: 3900f, EtaOverLimit: true);

        // when
        string text = StatusText.RunClock(clock, withElapsed: false, AnyColors);

        // then
        Assert.Equal("<color=#FF9900>ETA 1:05:00</color>", text);
    }
}

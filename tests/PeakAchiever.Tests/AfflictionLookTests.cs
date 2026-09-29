using PeakAchiever.Hud;
using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class AfflictionLookTests
{
    [Fact]
    public void A_rate_takes_the_look_of_the_affliction_it_measures()
    {
        // given
        var progress = new Progress(4, 10, ProgressScope.ThisRun, ProgressUnit.Percent, CharacterAfflictions.STATUSTYPE.Cold);

        // when
        CharacterAfflictions.STATUSTYPE? look = AfflictionBar.LookOf(progress);

        // then
        Assert.Equal(CharacterAfflictions.STATUSTYPE.Cold, look);
    }

    [Fact]
    public void The_run_time_takes_the_neutral_arrow_look()
    {
        // given
        var progress = new Progress(2533, 3600, ProgressScope.ThisRun, ProgressUnit.Duration);

        // when
        CharacterAfflictions.STATUSTYPE? look = AfflictionBar.LookOf(progress);

        // then
        Assert.Equal(CharacterAfflictions.STATUSTYPE.Arrow, look);
    }

    [Fact]
    public void A_counter_keeps_the_yellow_bar()
    {
        // given
        var progress = new Progress(3, 5, ProgressScope.ThisRun);

        // when
        CharacterAfflictions.STATUSTYPE? look = AfflictionBar.LookOf(progress);

        // then
        Assert.Null(look);
    }
}

using PeakAchiever.Tracking;

namespace PeakAchiever.Tests;

public class ProgressTests
{
    [Theory]
    [InlineData(7, 10, false)]
    [InlineData(8, 10, true)]
    [InlineData(14, 20, false)]
    [InlineData(15, 20, true)]
    public void A_rate_is_near_its_limit_from_three_quarters_on(int current, int target, bool near)
    {
        // given
        var progress = new Progress(current, target, ProgressScope.ThisRun, ProgressUnit.Percent);

        // when
        bool nearLimit = progress.NearLimit;

        // then
        Assert.Equal(near, nearLimit);
    }

    [Theory]
    [InlineData(ProgressUnit.Count)]
    [InlineData(ProgressUnit.Duration)]
    // ProgressUnit is internal, so a public theory takes it as object.
    public void Only_a_rate_has_a_limit_to_warn_about(object unit)
    {
        // given
        var progress = new Progress(9, 10, ProgressScope.ThisRun, (ProgressUnit)unit);

        // when
        bool nearLimit = progress.NearLimit;

        // then
        Assert.False(nearLimit);
    }
}

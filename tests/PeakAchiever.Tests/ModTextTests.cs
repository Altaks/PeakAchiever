using PeakAchiever.Localization;

namespace PeakAchiever.Tests;

public class ModTextTests
{
    [Fact]
    public void Every_key_has_a_non_empty_string_in_every_language()
    {
        // given
        int languageCount = Enum.GetValues<ModText.ModLanguage>().Length;

        // when
        ModTextKey[] incomplete = Enum.GetValues<ModTextKey>()
            .Where(key =>
                !ModText.Table.TryGetValue(key, out string[]? row)
                || row.Length != languageCount
                || row.Any(string.IsNullOrWhiteSpace)
            )
            .ToArray();

        // then
        Assert.Empty(incomplete);
    }
}

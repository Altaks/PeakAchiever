using System.Text.RegularExpressions;
using PeakAchiever.Localization;

namespace PeakAchiever.Tests;

public class ModTextTests
{
    [Fact]
    public void Every_key_has_a_non_empty_string_in_every_language()
    {
        // given
        int languageCount = Enum.GetValues<LocalizedText.Language>().Length;

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

    [Fact]
    public void Every_translation_keeps_the_english_placeholders()
    {
        // given
        static string Placeholders(string text) =>
            string.Join(",", Regex.Matches(text, @"\{\d+\}").Select(match => match.Value).Order());

        // when
        string[] mismatched = ModText.Table
            .SelectMany(row => row.Value.Select((text, language) => (row.Key, language, text)))
            .Where(cell => Placeholders(cell.text) != Placeholders(ModText.Table[cell.Key][0]))
            .Select(cell => $"{cell.Key} in {(LocalizedText.Language)cell.language}")
            .ToArray();

        // then
        Assert.Empty(mismatched);
    }
}

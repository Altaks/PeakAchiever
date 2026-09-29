using System;
using System.Linq;

namespace PeakAchiever.Localization;

/// <summary>
/// Puts mod strings into the game's own localization table, for the game's elements the mod clones:
/// their LocalizedText then translates them like any other (LocalizedText.GetText reads mainTable, v2.4.c).
/// </summary>
internal static class GameTextTable
{
    /// <summary>Called whenever the element shows: the game rebuilds its table when it reloads the texts.</summary>
    public static void Register(string gameKey, ModTextKey text)
    {
        int languages = Enum.GetValues(typeof(LocalizedText.Language)).Length;
        var texts = Enumerable.Repeat(ModText.In(text, ModText.ModLanguage.English), languages).ToList();
        texts[(int)LocalizedText.Language.French] = ModText.In(text, ModText.ModLanguage.French);
        LocalizedText.mainTable[gameKey] = texts;
    }
}

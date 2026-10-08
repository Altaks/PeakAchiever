using System.Linq;

namespace PeakAchiever.Localization;

/// <summary>
/// Puts mod strings into the game's own localization table, for the game's elements the mod clones:
/// their LocalizedText then translates them like any other (LocalizedText.GetText reads mainTable, v2.4.c).
/// </summary>
internal static class GameTextTable
{
    /// <summary>Called whenever the element shows: the game rebuilds its table when it reloads the texts.</summary>
    public static void Register(string gameKey, ModTextKey text) =>
        // A row already follows LocalizedText.Language's order, as the game's own rows do.
        LocalizedText.mainTable[gameKey] = ModText.Table[text].ToList();
}

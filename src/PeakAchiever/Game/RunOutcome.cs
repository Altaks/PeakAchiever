namespace PeakAchiever.Game;

/// <summary>Whether the current run was lost: its end screen opened with nobody at the summit.</summary>
internal static class RunOutcome
{
    public static bool Lost { get; private set; }

    /// <summary>
    /// Called as the end screen opens. The game sets the local scout's won and somebodyElseWon flags
    /// before it (EndScreen.EndSequenceRoutine picks its banner from them, v2.4.c).
    /// </summary>
    public static void RunEnded()
    {
        CharacterStats stats = Character.localCharacter.refs.stats;
        Lost = !stats.won && !stats.somebodyElseWon;
    }

    public static void RunStarted() => Lost = false;
}

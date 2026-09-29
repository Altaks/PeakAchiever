using System;
using HarmonyLib;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>Tells the tracker when something it shows may have changed, and when a new run starts.</summary>
[HarmonyPatch]
internal static class RunTrackingPatches
{
    // AchievementManager raises this after every run counter change, every collection addition
    // and every badge unlock (AchievementManager.cs, Player.cs; game v2.4.c).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.OnAchievementProgressChanged), new Type[0])]
    private static void AfterRunProgress() => Plugin.Hud.RequestRefresh();

    // Lifetime stats (meals cooked, height climbed...) change here without raising the event above.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.IncrementSteamStat))]
    private static void AfterStatIncremented() => Plugin.Hud.RequestRefresh();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.SetSteamStat))]
    private static void AfterStatSet() => Plugin.Hud.RequestRefresh();

    // Moving to the next segment can leave a biome behind.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapHandler), nameof(MapHandler.GoToSegment))]
    private static void AfterSegmentChanged() => Plugin.Hud.RequestRefresh();

    // EndScreen.EndSequenceRoutine calls this only when the local scout won, after the run timer
    // stopped: reaching the summit ends the last biome, which the periodic refresh never sees finish.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.TestTimeAchievements))]
    private static void AfterRunWon()
    {
        if (!RunFactsReader.IsInRun)
        {
            Plugin.Log.LogWarning("The run was won outside a readable run; its last biome time is not recorded.");
            return;
        }
        Plugin.Splits.RecordFinished(RunFactsReader.Read(Plugin.Splits.History).BiomeSplits, runEnded: true);
    }

    // The end screen opens once the run is over, won or lost (EndScreen.Start, v2.4.c).
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndScreen), "Start")]
    private static void AfterRunEnded()
    {
        if (Character.localCharacter == null)
            return;
        RunOutcome.RunEnded();
        Plugin.Hud.RequestRefresh();
    }

    // CharacterSpawner resets the run-based values through this overload when a run begins.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.InitRunBasedValues), typeof(SerializableRunBasedValues))]
    private static void AfterRunStarted()
    {
        AchievementManager achievements = Singleton<AchievementManager>.Instance;
        if (Plugin.Pins.Board.DropEarned(achievements.IsAchievementUnlocked))
            Plugin.Pins.Save();
        Plugin.Splits.StartRun();
        RunOutcome.RunStarted();
        Plugin.Hud.RequestRefresh();
    }
}

using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;

namespace PeakAchiever.Hud;

/// <summary>Turns a tracked status into the words under a card.</summary>
internal static class StatusText
{
    private const string BiomeNameSeparator = " / ";
    private const string SplitSeparator = " · ";
    private const string LineBreak = "\n";
    private const int SecondsPerMinute = 60;
    private const int SecondsPerHour = 3600;

    // Keys of the game's localization table holding each biome's display name (Localized_Text.csv, v2.4.c).
    private static readonly Dictionary<Biome.BiomeType, string> BiomeNameKeys = new()
    {
        [Biome.BiomeType.Shore] = "SHORE",
        [Biome.BiomeType.Tropics] = "TROPICS",
        [Biome.BiomeType.Alpine] = "ALPINE",
        [Biome.BiomeType.Volcano] = "CALDERA",
        [Biome.BiomeType.Peak] = "PEAK",
        [Biome.BiomeType.Mesa] = "MESA",
        [Biome.BiomeType.Roots] = "ROOTS",
        // The Gloom's segment is typed Swamp (MountainProgressHandler progress point "GLOOM", biome 8).
        [Biome.BiomeType.Swamp] = "GLOOM",
    };

    private static readonly Dictionary<BrokenCondition, ModTextKey> BrokenConditionKeys = new()
    {
        [BrokenCondition.TookFallDamage] = ModTextKey.ReasonTookFallDamage,
        [BrokenCondition.AtePackagedFood] = ModTextKey.ReasonAtePackagedFood,
        [BrokenCondition.PassedOut] = ModTextKey.ReasonPassedOut,
        [BrokenCondition.PlacedPermanentItem] = ModTextKey.ReasonPlacedPermanentItem,
        [BrokenCondition.RunTooLong] = ModTextKey.ReasonRunTooLong,
        [BrokenCondition.NotSolo] = ModTextKey.ReasonNotSolo,
        [BrokenCondition.TooMuchHeat] = ModTextKey.ReasonTooMuchHeat,
        [BrokenCondition.TooMuchCold] = ModTextKey.ReasonTooMuchCold,
        [BrokenCondition.TooManySpores] = ModTextKey.ReasonTooManySpores,
        [BrokenCondition.HitByTrap] = ModTextKey.ReasonHitByTrap,
    };

    public static string Describe(UnattainableReason reason) =>
        reason switch
        {
            UnattainableReason.BiomeAbsent absent => ModText.Format(ModTextKey.ReasonBiomeAbsent, BiomeNames(absent.Biomes)),
            UnattainableReason.BiomeLeft left => ModText.Format(ModTextKey.ReasonBiomeLeft, BiomeNames(left.Biomes)),
            UnattainableReason.ConditionBroken broken => ModText.Get(BrokenConditionKeys[broken.Condition]),
            _ => throw new System.ArgumentOutOfRangeException(nameof(reason), reason, "Unhandled unattainable reason."),
        };

    public static string Count(Progress progress) =>
        progress.Unit switch
        {
            ProgressUnit.Count => $"{progress.Current} / {progress.Target}",
            ProgressUnit.Duration => $"{Clock(progress.Current)} / {Clock(progress.Target)}",
            ProgressUnit.Percent => ModText.Format(ModTextKey.LimitRate, progress.Current, progress.Target),
            _ => throw new System.ArgumentOutOfRangeException(nameof(progress), progress.Unit, "Unhandled progress unit."),
        };

    /// <summary>Hours, minutes and seconds, as the game's end screen writes them (EndScreen.GetTimeString, v2.4.c).</summary>
    public static string Clock(float seconds)
    {
        int whole = (int)System.Math.Floor(seconds);
        return $"{whole / SecondsPerHour}:{whole % SecondsPerHour / SecondsPerMinute:00}:{whole % SecondsPerMinute:00}";
    }

    /// <summary>
    /// The run clock's lines under the status row: the elapsed time (once the bar is gone) and the ETA,
    /// then the biome splits. Empty until there is any of them.
    /// </summary>
    public static string RunClock(BadgeDetail.RunClock clock, bool withElapsed, string currentColor)
    {
        var head = new List<string>();
        if (withElapsed)
            head.Add(Clock(clock.ElapsedSeconds));
        if (clock.EtaSeconds is { } eta)
            head.Add(ModText.Format(ModTextKey.Eta, Clock(eta)));
        string[] lines = [string.Join(SplitSeparator, head), Splits(clock.Splits, currentColor)];
        return string.Join(LineBreak, lines.Where(line => line.Length > 0));
    }

    /// <summary>Each biome with its time, the one still counting in the progress colour.</summary>
    public static string Splits(IEnumerable<BiomeSplit> splits, string currentColor) =>
        string.Join(
            SplitSeparator,
            splits.Select(split =>
            {
                string text = $"{BiomeName(split.Biome)} {Clock(split.Seconds)}";
                return split.IsCurrent ? $"<color=#{currentColor}>{text}</color>" : text;
            })
        );

    private static string BiomeNames(Biome.BiomeType[] biomes) => string.Join(BiomeNameSeparator, biomes.Select(BiomeName));

    private static string BiomeName(Biome.BiomeType biome) =>
        BiomeNameKeys.TryGetValue(biome, out string key) ? LocalizedText.GetText(key) : biome.ToString();
}

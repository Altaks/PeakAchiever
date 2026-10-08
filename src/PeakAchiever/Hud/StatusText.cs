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
    // Where the time and the gap of each split line start, as TextMeshPro <pos> offsets of the line.
    public const string SplitTimeColumn = "48%";
    public const string SplitDeltaColumn = "74%";
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
        // The Nadir, reached through an item; its name has its own key (Localized_Text.csv, v2.6.b).
        [Biome.BiomeType.Void] = "AREA_VOID",
    };

    private static readonly Dictionary<BrokenCondition, ModTextKey> BrokenConditionKeys = new()
    {
        [BrokenCondition.TookFallDamage] = ModTextKey.ReasonTookFallDamage,
        [BrokenCondition.AtePackagedFood] = ModTextKey.ReasonAtePackagedFood,
        [BrokenCondition.PassedOut] = ModTextKey.ReasonPassedOut,
        [BrokenCondition.PlacedPermanentItem] = ModTextKey.ReasonPlacedPermanentItem,
        [BrokenCondition.RunTooLong] = ModTextKey.ReasonRunTooLong,
        [BrokenCondition.TooMuchHeat] = ModTextKey.ReasonTooMuchHeat,
        [BrokenCondition.TooMuchCold] = ModTextKey.ReasonTooMuchCold,
        [BrokenCondition.TooManySpores] = ModTextKey.ReasonTooManySpores,
        [BrokenCondition.HitByTrap] = ModTextKey.ReasonHitByTrap,
        [BrokenCondition.RunLost] = ModTextKey.ReasonRunLost,
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
            // The limit is in the badge's own description.
            ProgressUnit.Duration => Clock(progress.Current),
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
    /// The run clock's lines under the status row: once the bar is gone, the elapsed time and the ETA; then
    /// the biome splits, one a line. Empty until there is any of them. While the bar shows, the ETA sits
    /// beside it (<see cref="Eta"/>).
    /// </summary>
    public static string RunClock(BadgeDetail.RunClock clock, bool withElapsed, ClockColors colors)
    {
        var head = new List<string>();
        if (withElapsed)
        {
            head.Add(Clock(clock.ElapsedSeconds));
            if (Eta(clock, colors) is { Length: > 0 } eta)
                head.Add(eta);
        }
        string[] lines = [string.Join(SplitSeparator, head), .. clock.Splits.Select(compared => SplitLine(compared, colors))];
        return string.Join(LineBreak, lines.Where(line => line.Length > 0));
    }

    /// <summary>When past runs say the summit should be reached: within the limit in the faster colour, past it in caution.</summary>
    public static string Eta(BadgeDetail.RunClock clock, ClockColors colors) =>
        clock.EtaSeconds is { } eta
            ? $"<color=#{(clock.EtaOverLimit ? colors.Caution : colors.Faster)}>{ModText.Format(ModTextKey.Eta, Clock(eta))}</color>"
            : "";

    /// <summary>
    /// One biome, its time and its gap to the median in columns; the biome still counting in the current
    /// colour, the gap signed and coloured (the sign carries it without the colour).
    /// </summary>
    private static string SplitLine(ComparedSplit compared, ClockColors colors)
    {
        BiomeSplit split = compared.Split;
        string text = $"{BiomeName(split.Biome)}<pos={SplitTimeColumn}>{Clock(split.Seconds)}";
        if (split.IsCurrent)
            text = $"<color=#{colors.Current}>{text}</color>";
        if (compared.DeltaSeconds is { } delta)
            text += $"<pos={SplitDeltaColumn}><color=#{(delta > 0f ? colors.Slower : colors.Faster)}>{Delta(delta)}</color>";
        return text;
    }

    /// <summary>A signed gap: minutes and seconds, hours only from one hour on.</summary>
    public static string Delta(float seconds)
    {
        int whole = (int)System.Math.Floor(System.Math.Abs(seconds));
        string sign = seconds < 0f ? "-" : "+";
        int minutes = whole % SecondsPerHour / SecondsPerMinute;
        int rest = whole % SecondsPerMinute;
        return whole >= SecondsPerHour ? $"{sign}{whole / SecondsPerHour}:{minutes:00}:{rest:00}" : $"{sign}{minutes}:{rest:00}";
    }

    public static string BiomeNames(Biome.BiomeType[] biomes) => string.Join(BiomeNameSeparator, biomes.Select(BiomeName));

    public static string BiomeName(Biome.BiomeType biome) =>
        BiomeNameKeys.TryGetValue(biome, out string key) ? LocalizedText.GetText(key) : biome.ToString();
}

/// <summary>Rich-text colours of the run clock lines, as hex RGB.</summary>
internal readonly record struct ClockColors(string Current, string Slower, string Faster, string Caution);

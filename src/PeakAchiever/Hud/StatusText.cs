using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Localization;
using PeakAchiever.Tracking;

namespace PeakAchiever.Hud;

/// <summary>Turns a tracked status into the words under a card.</summary>
internal static class StatusText
{
    private const string BiomeNameSeparator = " / ";

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

    public static string Count(Progress progress) => $"{progress.Current} / {progress.Target}";

    private static string BiomeNames(Biome.BiomeType[] biomes) =>
        string.Join(
            BiomeNameSeparator,
            biomes.Select(biome => BiomeNameKeys.TryGetValue(biome, out string key) ? LocalizedText.GetText(key) : biome.ToString())
        );
}

using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>A map the badges page can judge badges against: the run's own, or today's from the airport.</summary>
internal readonly record struct KnownMap(IReadOnlyCollection<Biome.BiomeType> Biomes, bool IsToday);

/// <summary>The maps the game can pick, read from its level table.</summary>
internal static class MapCatalog
{
    private static IReadOnlyCollection<Biome.BiomeType>[]? _levels;
    private static BadgeCompatibility? _compatibility;

    /// <summary>Which badges can share a run, going by every level; unconstrained while the table is not loaded.</summary>
    public static BadgeCompatibility Compatibility
    {
        get
        {
            if (_compatibility != null)
                return _compatibility;
            if (Levels is not { } levels)
            {
                Plugin.Log.LogWarning("The game's level table is not loaded; no badge is refused as incompatible.");
                return BadgeCompatibility.Unconstrained;
            }
            return _compatibility = new BadgeCompatibility(levels);
        }
    }

    /// <summary>The distinct biome layouts of the levels, in the table's order; empty while it is not loaded.</summary>
    public static IReadOnlyList<IReadOnlyCollection<Biome.BiomeType>> Layouts =>
        Levels is { } levels
            ? levels.GroupBy(level => string.Join("/", level)).Select(layout => layout.First()).ToArray()
            : [];

    /// <summary>
    /// The run's map during a run; in the airport, the level the check-in kiosk sends today
    /// (AirportCheckInKiosk: NextLevelIndexOrFallback + debugLevelIndexOffset, wrapped over the levels,
    /// v2.4.c). Null anywhere else, or while the level table is not loaded.
    /// </summary>
    public static KnownMap? CurrentOrToday
    {
        get
        {
            if (RunFactsReader.IsInRun)
                return new KnownMap(Singleton<MapHandler>.Instance.biomes.ToArray(), IsToday: false);
            if (Character.localCharacter == null || !Character.localCharacter.inAirport || Levels is not { } levels)
                return null;
            int index = GameHandler.GetService<NextLevelService>().NextLevelIndexOrFallback + NextLevelService.debugLevelIndexOffset;
            return new KnownMap(levels[(index % levels.Length + levels.Length) % levels.Length], IsToday: true);
        }
    }

    /// <summary>
    /// The biomes of each generated level, in climbing order, from MapBaker.selectedBiomes (PEAK 2.4.c:
    /// 25 levels, alternating Shore / Tropics / Alpine / Volcano and Shore / Roots / Mesa / Swamp, read
    /// from data.unity3d with UnityPy). Read once it has loaded, then cached.
    /// </summary>
    private static IReadOnlyCollection<Biome.BiomeType>[]? Levels
    {
        get
        {
            if (_levels != null)
                return _levels;
            MapBaker? baker = SingletonAsset<MapBaker>.Instance;
            if (baker == null || baker.selectedBiomes == null || baker.selectedBiomes.Count == 0)
                return null;
            _levels = baker.selectedBiomes.Select(level => (IReadOnlyCollection<Biome.BiomeType>)level.biomeTypes.ToArray()).ToArray();
            IEnumerable<string> layouts = _levels.Select(level => string.Join("/", level)).Distinct();
            Plugin.Log.LogInfo($"{_levels.Length} levels, map layouts: {string.Join(", ", layouts)}");
            return _levels;
        }
    }
}

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
    private static BadgeCompatibility? _compatibility;

    /// <summary>
    /// Which badges can share a run. Built once from MapBaker.selectedBiomes, the biomes of each
    /// generated level in climbing order (PEAK 2.4.c: 25 levels, alternating Shore / Tropics / Alpine /
    /// Volcano and Shore / Roots / Mesa / Swamp, read from data.unity3d with UnityPy).
    /// </summary>
    public static BadgeCompatibility Compatibility
    {
        get
        {
            if (_compatibility != null)
                return _compatibility;
            MapBaker? baker = SingletonAsset<MapBaker>.Instance;
            if (baker == null || baker.selectedBiomes == null || baker.selectedBiomes.Count == 0)
            {
                Plugin.Log.LogWarning("The game's level table is not loaded; no badge is refused as incompatible.");
                return BadgeCompatibility.Unconstrained;
            }
            IReadOnlyCollection<Biome.BiomeType>[] maps = baker
                .selectedBiomes.Select(level => (IReadOnlyCollection<Biome.BiomeType>)level.biomeTypes.ToArray())
                .ToArray();
            IEnumerable<string> layouts = maps.Select(map => string.Join("/", map)).Distinct();
            Plugin.Log.LogInfo($"{maps.Length} levels, map layouts: {string.Join(", ", layouts)}");
            return _compatibility = new BadgeCompatibility(maps);
        }
    }
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
            if (Character.localCharacter == null || !Character.localCharacter.inAirport)
                return null;
            MapBaker? baker = SingletonAsset<MapBaker>.Instance;
            if (baker == null || baker.selectedBiomes == null || baker.selectedBiomes.Count == 0)
                return null;
            int levels = baker.selectedBiomes.Count;
            int index = GameHandler.GetService<NextLevelService>().NextLevelIndexOrFallback + NextLevelService.debugLevelIndexOffset;
            return new KnownMap(baker.selectedBiomes[(index % levels + levels) % levels].biomeTypes.ToArray(), IsToday: true);
        }
    }
}

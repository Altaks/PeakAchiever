using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using Zorro.Core;

namespace PeakAchiever.Game;

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
}

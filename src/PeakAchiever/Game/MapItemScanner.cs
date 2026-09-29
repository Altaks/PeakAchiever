using System;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using UnityEngine;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>
/// The items the loaded level can yield, read once per run from its spawners and the items lying in it.
/// Every doubt widens the set, since an item left out would read as absent from the map.
/// </summary>
internal static class MapItemScanner
{
    private static readonly SpawnPool[] SinglePools = Enum.GetValues(typeof(SpawnPool)).Cast<SpawnPool>().Where(pool => pool != SpawnPool.None).ToArray();

    private static MapHandler? _scannedMap;
    private static IReadOnlyCollection<ushort>? _items;

    /// <summary>Null when the level could not be read; the checklist then marks nothing as absent.</summary>
    public static IReadOnlyCollection<ushort>? ItemsOnMap
    {
        get
        {
            MapHandler map = Singleton<MapHandler>.Instance;
            if (_scannedMap == map)
                return _items;
            _scannedMap = map;
            _items = Scan();
            return _items;
        }
    }

    private static IReadOnlyCollection<ushort>? Scan()
    {
        try
        {
            var direct = new HashSet<ushort>();
            // FindObjectsOfTypeAll also finds the inactive segments not reached yet; the scene check
            // leaves out prefab assets, which belong to no level.
            foreach (Spawner spawner in Resources.FindObjectsOfTypeAll<Spawner>().Where(InLevel))
            {
                // What Spawner.GetObjectsToSpawn and BerryBush.SpawnItems can pick from (v2.4.c).
                AddPool(direct, spawner.spawnPool);
                foreach (Spawner.HeightBasedSpawnListEntry entry in spawner.heightBasedSpawnPools ?? [])
                    AddPool(direct, entry.spawnPool);
                AddPrefab(direct, spawner.spawnedObjectPrefab);
                AddPrefab(direct, spawner.fallbackSpawn);
                foreach (SpawnEntry entry in spawner.spawns != null ? spawner.spawns.items : [])
                    AddPrefab(direct, entry.prefab);
            }
            foreach (Item item in Resources.FindObjectsOfTypeAll<Item>().Where(InLevel))
                direct.Add(item.itemID);
            IReadOnlyCollection<ushort> items = ItemSources.Reachable(direct, SpawnsOnUse());
            Plugin.Log.LogInfo($"This map can yield {items.Count} kinds of item.");
            return items;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Could not read this map's items; the checklists mark none as absent. {e}");
            return null;
        }
    }

    private static bool InLevel(Component component) => component.gameObject.scene.IsValid();

    // LootData indexes each pool flag on its own (LootData.PopulateLootData); a spawner set to several
    // flags gets every one of them here, which can only widen the set.
    private static void AddPool(HashSet<ushort> items, SpawnPool pool)
    {
        foreach (SpawnPool single in SinglePools.Where(single => pool.HasFlag(single)))
        {
            foreach (Item item in LootData.GetAllItemsInPool(single))
                items.Add(item.itemID);
        }
    }

    private static void AddPrefab(HashSet<ushort> items, GameObject? prefab)
    {
        if (prefab != null && prefab.TryGetComponent(out Item item))
            items.Add(item.itemID);
    }

    // Items that turn into others when used: Action_Spawn.objectToSpawn, Action_ConsumeAndSpawn.itemToSpawn.
    private static Dictionary<ushort, IReadOnlyCollection<ushort>> SpawnsOnUse()
    {
        var spawnsOnUse = new Dictionary<ushort, IReadOnlyCollection<ushort>>();
        foreach (KeyValuePair<ushort, Item> entry in SingletonAsset<ItemDatabase>.Instance.itemLookup)
        {
            var spawned = new HashSet<ushort>();
            foreach (Action_Spawn action in entry.Value.GetComponentsInChildren<Action_Spawn>(includeInactive: true))
                AddPrefab(spawned, action.objectToSpawn);
            foreach (Action_ConsumeAndSpawn action in entry.Value.GetComponentsInChildren<Action_ConsumeAndSpawn>(includeInactive: true))
            {
                if (action.itemToSpawn != null)
                    spawned.Add(action.itemToSpawn.itemID);
            }
            if (spawned.Count > 0)
                spawnsOnUse[entry.Key] = spawned;
        }
        return spawnsOnUse;
    }
}

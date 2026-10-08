using System;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Tracking;
using UnityEngine;
using Zorro.Core;

namespace PeakAchiever.Game;

/// <summary>The game's items as the eating badges see them, read from its item database.</summary>
internal static class ItemCatalog
{
    // The game marks items it left in the database but no longer uses so (Clusterberry_UNUSED, v2.4.c).
    private const string UnusedItemSuffix = "_UNUSED";

    private static readonly IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>> NoCandidates =
        new Dictionary<RunCollection, IReadOnlyList<ushort>>();
    private static IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>>? _candidates;

    /// <summary>
    /// Every item id that eating would add to each run list, in id order. Read once the database has
    /// loaded, then cached.
    /// </summary>
    public static IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>> CollectionCandidates
    {
        get
        {
            if (_candidates != null)
                return _candidates;
            if (SingletonAsset<ItemDatabase>.Instance.itemLookup.Count == 0)
            {
                Plugin.Log.LogWarning("The item database is still empty; the checklists wait for it.");
                return NoCandidates;
            }
            return _candidates = ReadCandidates();
        }
    }

    public static Texture? Icon(ushort itemId) => ItemDatabase.TryGetItem(itemId, out Item item) ? item.UIData.icon : null;

    private static Dictionary<string, Item>? _byName;
    private static readonly HashSet<string> ReportedMissing = [];

    /// <summary>
    /// The item whose Item.UIData.itemName is <paramref name="itemName"/>, ignoring case; null, reported once,
    /// when the game has none (renamed in an update) or the database is not loaded yet.
    /// </summary>
    public static Item? ByName(string itemName)
    {
        if (_byName == null)
        {
            if (SingletonAsset<ItemDatabase>.Instance.itemLookup.Count == 0)
                return null;
            _byName = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
            foreach (Item item in SingletonAsset<ItemDatabase>.Instance.itemLookup.OrderBy(entry => entry.Key).Select(entry => entry.Value))
            {
                if (item.UIData?.itemName is { Length: > 0 } name && !_byName.ContainsKey(name))
                    _byName[name] = item;
            }
        }
        if (_byName.TryGetValue(itemName, out Item found))
            return found;
        if (ReportedMissing.Add(itemName))
            Plugin.Log.LogWarning($"The game has no item named '{itemName}'; its icon is left off the badge cards.");
        return null;
    }

    private static IReadOnlyDictionary<RunCollection, IReadOnlyList<ushort>> ReadCandidates()
    {
        var candidates = new Dictionary<RunCollection, List<ushort>>();
        foreach (KeyValuePair<ushort, Item> entry in SingletonAsset<ItemDatabase>.Instance.itemLookup.OrderBy(entry => entry.Key))
        {
            if (entry.Value.name.EndsWith(UnusedItemSuffix, StringComparison.Ordinal))
                continue;
            // AchievementManager.TestItemConsumed tells a poisonous mushroom by this action on its prefab (v2.4.c).
            bool poisons = entry.Value.GetComponent<Action_InflictPoison>() != null;
            foreach (RunCollection collection in RunCollections.CountingItem(entry.Value.itemTags, poisons))
            {
                if (!candidates.TryGetValue(collection, out List<ushort> items))
                    candidates[collection] = items = [];
                items.Add(entry.Key);
            }
        }
        foreach (KeyValuePair<RunCollection, List<ushort>> list in candidates)
        {
            IEnumerable<string> names = list.Value.Select(id => SingletonAsset<ItemDatabase>.Instance.itemLookup[id].name);
            Plugin.Log.LogInfo($"{list.Key}: {list.Value.Count} items ({string.Join(", ", names)})");
        }
        return candidates.ToDictionary(list => list.Key, list => (IReadOnlyList<ushort>)list.Value);
    }
}

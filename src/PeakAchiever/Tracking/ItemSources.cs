using System.Collections.Generic;

namespace PeakAchiever.Tracking;

internal static class ItemSources
{
    /// <summary>
    /// Every item a map can yield: those its spawners and scene hold, and whatever those turn into when
    /// used (Action_Spawn, Action_ConsumeAndSpawn), however many steps away.
    /// </summary>
    /// <param name="direct">The items the map's spawners and scene hold.</param>
    /// <param name="spawnsOnUse">For each item, the items using it spawns.</param>
    public static IReadOnlyCollection<ushort> Reachable(
        IEnumerable<ushort> direct,
        IReadOnlyDictionary<ushort, IReadOnlyCollection<ushort>> spawnsOnUse
    )
    {
        var reached = new HashSet<ushort>();
        var toVisit = new Stack<ushort>(direct);
        while (toVisit.Count > 0)
        {
            ushort item = toVisit.Pop();
            if (!reached.Add(item) || !spawnsOnUse.TryGetValue(item, out IReadOnlyCollection<ushort> spawned))
                continue;
            foreach (ushort next in spawned)
                toVisit.Push(next);
        }
        return reached;
    }
}

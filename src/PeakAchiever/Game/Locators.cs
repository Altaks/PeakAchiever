using System.Collections.Generic;
using System.Linq;
using Peak;
using PeakAchiever.Localization;
using UnityEngine;

namespace PeakAchiever.Game;

/// <summary>What a badge's locator points at, and how it names it.</summary>
internal enum LocatorTarget
{
    UnlitBell,
    Antlion,
    Tomb,
}

/// <summary>
/// Finds, in the loaded scene, the thing a badge is earned at: the nearest unlit belltower (Bellringer), the
/// nearest antlion (Megaentomology), the Mesa tomb (24 Karat). The scene is searched once a second at most;
/// the positions are read every frame.
/// </summary>
internal static class Locators
{
    private const float RescanSeconds = 1f;

    private static readonly Dictionary<ACHIEVEMENTTYPE, LocatorTarget> Targets = new()
    {
        [ACHIEVEMENTTYPE.BellringerBadge] = LocatorTarget.UnlitBell,
        [ACHIEVEMENTTYPE.MegaentomologyBadge] = LocatorTarget.Antlion,
        [ACHIEVEMENTTYPE.TwentyFourKaratBadge] = LocatorTarget.Tomb,
    };

    private static readonly Dictionary<LocatorTarget, ModTextKey> Names = new()
    {
        [LocatorTarget.UnlitBell] = ModTextKey.LocatorBell,
        [LocatorTarget.Antlion] = ModTextKey.LocatorAntlion,
        [LocatorTarget.Tomb] = ModTextKey.LocatorTomb,
    };

    private static float _nextScan;
    private static Antlion[] _antlions = [];
    private static TombTrigger? _tomb;

    public static LocatorTarget? TargetOf(ACHIEVEMENTTYPE badge) => Targets.TryGetValue(badge, out LocatorTarget target) ? target : null;

    public static string NameOf(LocatorTarget target) => ModText.Get(Names[target]);

    /// <summary>Where the badge's target is, the nearest to <paramref name="from"/>; null when the loaded scene has none.</summary>
    public static Vector3? Find(LocatorTarget target, Vector3 from)
    {
        Rescan();
        IEnumerable<Vector3> candidates = target switch
        {
            // The bells register themselves while their segment is loaded (GloomSafeZone.OnEnable, v2.6.b).
            LocatorTarget.UnlitBell => GloomSafeZone.ALL_GLOOM_SAFE_ZONES.OfType<GhostFire>().Where(bell => bell != null && !bell.isLit).Select(bell => bell.transform.position),
            // Antlion.TestAchievement grants the badge to a scout it bit who gets far enough away (v2.6.b).
            LocatorTarget.Antlion => _antlions.Where(antlion => antlion != null && antlion.isActiveAndEnabled).Select(antlion => antlion.transform.position),
            // TombTrigger marks the tomb's entrance (v2.6.b).
            LocatorTarget.Tomb => _tomb != null && _tomb.isActiveAndEnabled ? [_tomb.transform.position] : [],
            _ => throw new System.ArgumentOutOfRangeException(nameof(target), target, "Unhandled locator target."),
        };
        Vector3? nearest = null;
        float best = float.MaxValue;
        foreach (Vector3 position in candidates)
        {
            float distance = (position - from).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                nearest = position;
            }
        }
        return nearest;
    }

    // Scene objects with no list of their own are searched, not every frame: the search walks the whole scene.
    private static void Rescan()
    {
        if (Time.unscaledTime < _nextScan)
            return;
        _nextScan = Time.unscaledTime + RescanSeconds;
        _antlions = Object.FindObjectsByType<Antlion>(FindObjectsSortMode.None);
        _tomb = Object.FindFirstObjectByType<TombTrigger>();
    }
}

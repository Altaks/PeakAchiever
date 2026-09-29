using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Zorro.Core;
using PhotonPlayer = Photon.Realtime.Player;

namespace PeakAchiever.Team;

/// <summary>
/// What modded players share in a multiplayer game: each one's earned badges, as a player property, and
/// the host's team pins, as a room property. Read on every tracker refresh; nothing is pushed by event.
/// </summary>
/// <remarks>
/// Safe for players without the mod: only Photon custom properties, under keys of the mod's own, holding
/// plain strings. PEAK's own property handlers only look up their own keys (CachedPlayerList,
/// ReconnectHandler... just mark themselves dirty; reads go through named keys like "UserID"), and strings
/// need no custom serializer, v2.4.c. No RPC, no custom event, no networked object.
/// </remarks>
internal static class TeamSync
{
    private const string EarnedKey = "PeakAchiever.Earned";
    private const string TeamPinsKey = "PeakAchiever.TeamPins";

    private static readonly ACHIEVEMENTTYPE[] AllBadges = Enum.GetValues(typeof(ACHIEVEMENTTYPE))
        .Cast<ACHIEVEMENTTYPE>()
        .Where(badge => badge != ACHIEVEMENTTYPE.NONE)
        .ToArray();

    private static readonly HashSet<string> ReportedUnknown = [];
    private static string? _publishedEarned;
    // The team pins the host sent and the room has not echoed back yet, and when.
    private static string? _sentPins;
    private static float _sentAt;
    // A change the server never confirms stops showing after this long.
    private const float EchoTimeoutSeconds = 5f;

    /// <summary>In a room with other players' machines, not the game's offline solo mode.</summary>
    public static bool InMultiplayer => PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode;

    /// <summary>The host is Photon's master client; a new one takes over when the host leaves.</summary>
    public static bool IsHost => InMultiplayer && PhotonNetwork.IsMasterClient;

    /// <summary>The team pins the host set, in the host's order; empty outside multiplayer.</summary>
    public static IReadOnlyList<ACHIEVEMENTTYPE> TeamPins
    {
        get
        {
            if (!InMultiplayer)
                return [];
            string? stored = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TeamPinsKey, out object value) ? value as string : null;
            // The room keeps the old value until the server echoes a change back: until then the host
            // reads what it just set, so a click shows at once and a quick second one builds on it.
            if (_sentPins != null && stored != _sentPins && UnityEngine.Time.unscaledTime < _sentAt + EchoTimeoutSeconds)
                return Read(_sentPins);
            _sentPins = null;
            return stored == null ? [] : Read(stored);
        }
    }

    /// <summary>Every player in the room, their earned badges null when they do not share any.</summary>
    public static IReadOnlyList<Scout> Scouts =>
        InMultiplayer ? PhotonNetwork.PlayerList.Select(ScoutOf).ToArray() : [];

    /// <summary>Shares the local player's earned badges, when they changed or the room does not have them yet.</summary>
    public static void PublishEarned()
    {
        if (!InMultiplayer)
            return;
        // == null, not ?.: Unity objects override the null check.
        AchievementManager achievements = Singleton<AchievementManager>.Instance;
        if (achievements == null)
            return;
        string earned = BadgeNames.Join(AllBadges.Where(achievements.IsAchievementUnlocked));
        bool shared = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(EarnedKey);
        if (shared && earned == _publishedEarned)
            return;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [EarnedKey] = earned });
        _publishedEarned = earned;
    }

    /// <summary>Sets the team pins for everyone; only the host may.</summary>
    public static void SetTeamPins(IEnumerable<ACHIEVEMENTTYPE> pins)
    {
        if (!IsHost)
        {
            Plugin.Log.LogWarning("Only the host sets the team pins; the change is left out.");
            return;
        }
        string joined = BadgeNames.Join(pins);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [TeamPinsKey] = joined });
        _sentPins = joined;
        _sentAt = UnityEngine.Time.unscaledTime;
    }

    /// <summary>
    /// At the start of a run, the host drops the team pins every scout with the mod has now earned; the
    /// others stay for the next run.
    /// </summary>
    public static void DropEarnedByAll()
    {
        if (!IsHost)
            return;
        IReadOnlyList<ACHIEVEMENTTYPE> pins = TeamPins;
        IReadOnlyList<Scout> scouts = Scouts;
        ACHIEVEMENTTYPE[] kept = pins.Where(badge => !Team.Scouts.EarnedByAll(badge, scouts)).ToArray();
        if (kept.Length != pins.Count)
            SetTeamPins(kept);
    }

    private static Scout ScoutOf(PhotonPlayer player)
    {
        IReadOnlyCollection<ACHIEVEMENTTYPE>? earned =
            player.CustomProperties.TryGetValue(EarnedKey, out object value) && value is string text ? Read(text).ToHashSet() : null;
        return new Scout(player.NickName, player.IsLocal, earned);
    }

    // Names another version of the game knows and this one does not are left out, each reported once.
    private static IReadOnlyList<ACHIEVEMENTTYPE> Read(string text)
    {
        BadgeNames.Parsed parsed = BadgeNames.Parse(text);
        foreach (string name in parsed.Unknown.Where(ReportedUnknown.Add))
            Plugin.Log.LogWarning($"A player shared badge '{name}', which this game does not know; it is left out.");
        return parsed.Badges;
    }
}

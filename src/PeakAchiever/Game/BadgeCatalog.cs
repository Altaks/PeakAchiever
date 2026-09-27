using PeakAchiever.Localization;
using UnityEngine;

namespace PeakAchiever.Game;

/// <summary>What the tracker card shows for a badge, taken from the game's own badge data.</summary>
internal readonly record struct BadgePresentation(Texture? Icon, string Name, string Description, bool IsHidden);

internal static class BadgeCatalog
{
    /// <summary>
    /// Mirrors BadgeManager.selectedBadge: a locked secret badge keeps "???" for both texts.
    /// Unlike the game's popup, a locked non-secret badge shows its real name, since the player pinned it.
    /// </summary>
    public static BadgePresentation Present(ACHIEVEMENTTYPE badge)
    {
        BadgeData? data = GUIManager.instance.mainBadgeManager.GetBadgeData(badge);
        if (data == null)
        {
            Plugin.Log.LogWarning($"The game has no badge data for {badge}; showing its internal name.");
            return new BadgePresentation(null, badge.ToString(), "", IsHidden: false);
        }
        if (data.secret && data.IsLocked)
        {
            string placeholder = ModText.Get(ModTextKey.SecretPlaceholder);
            return new BadgePresentation(data.icon, placeholder, placeholder, IsHidden: true);
        }
        return new BadgePresentation(
            data.icon,
            LocalizedText.GetText(LocalizedText.GetNameIndex(data.displayName)),
            LocalizedText.GetText(LocalizedText.GetDescriptionIndex(data.displayName)),
            IsHidden: false
        );
    }
}

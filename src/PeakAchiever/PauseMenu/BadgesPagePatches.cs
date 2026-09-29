using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Localization;

namespace PeakAchiever.PauseMenu;

/// <summary>Makes the pause menu's badges page the place where badges get pinned.</summary>
[HarmonyPatch]
internal static class BadgesPagePatches
{
    // The pause menu's badge grid lives under PauseMenuAccoladesPage; the end screen reuses
    // BadgeUI and BadgeManager (EndScreen.cs), where pinning makes no sense.
    private static bool IsOnPauseMenu(UnityEngine.Component component) =>
        component.GetComponentInParent<PauseMenuAccoladesPage>(includeInactive: true) != null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BadgeUI), nameof(BadgeUI.Init))]
    private static void MakeBadgePinnable(BadgeUI __instance, BadgeData data)
    {
        // == null, not a pattern: Unity objects override the null check.
        PauseMenuAccoladesPage page = __instance.GetComponentInParent<PauseMenuAccoladesPage>(includeInactive: true);
        if (data == null || page == null)
            return;
        PinnableBadge.AttachTo(__instance);
        Plugin.Hud.Stats?.Watch(page);
    }

    /// <summary>Adds the click hint under the game's own description of a badge not yet earned.</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BadgeManager), nameof(BadgeManager.selectedBadge), MethodType.Setter)]
    private static void AppendPinHint(BadgeManager __instance)
    {
        BadgeUI selected = __instance.selectedBadge;
        if (selected == null || selected.data == null || !selected.data.IsLocked || !IsOnPauseMenu(__instance))
            return;
        ACHIEVEMENTTYPE badge = selected.data.linkedAchievement;
        string hint;
        if (Plugin.Pins.Board.IsPinned(badge))
            hint = ModText.Get(ModTextKey.HintClickToUnpin);
        else if (PinnableBadge.ConflictOf(badge) is { } conflict)
            hint = ModText.Format(ModTextKey.HintConflict, BadgeCatalog.Present(conflict).Name);
        else
            hint = ModText.Get(ModTextKey.HintClickToPin);
        if (PinnableBadge.Suggestions(__instance).Contains(badge) && MapCatalog.CurrentOrToday is { } suggestedFor)
        {
            ModTextKey why = suggestedFor.IsToday ? ModTextKey.HintSuggestedToday : ModTextKey.HintSuggestedThisMap;
            __instance.badgePopupDescription.text += $"\n<size=85%>{ModText.Get(why)}</size>";
        }
        IReadOnlyCollection<Biome.BiomeType> missing = PinnableBadge.MissingOnKnownMap(badge);
        if (missing.Count > 0 && MapCatalog.CurrentOrToday is { } map)
        {
            ModTextKey where = map.IsToday ? ModTextKey.HintNotOnTodaysMap : ModTextKey.HintNotOnThisMap;
            __instance.badgePopupDescription.text += $"\n<size=85%>{ModText.Format(where, StatusText.BiomeNames(missing.ToArray()))}</size>";
        }
        __instance.badgePopupDescription.text += $"\n<size=85%><b>{hint}</b></size>";
    }
}

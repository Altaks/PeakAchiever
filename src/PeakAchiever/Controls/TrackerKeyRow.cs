using System;
using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.ControllerSupport;
using Zorro.Core;
using Zorro.UI;
using Object = UnityEngine.Object;

namespace PeakAchiever.Controls;

/// <summary>
/// The tracker key on the game's Controls page: a clone of one of the page's own rows (its background,
/// key icon, label and reset button), placed at the end of the first column, that sends the mod's
/// action through the game's own rebinding page.
/// </summary>
internal sealed class TrackerKeyRow : MonoBehaviour
{
    private const string RowName = "PeakAchiever.TrackerKey";
    // The label goes through the game's localization table, so the cloned LocalizedText keeps
    // translating it, and PauseMenuRebindKeyPage can show it in its prompt (v2.4.c).
    private const string LabelKey = "PEAKACHIEVER_TOGGLE_TRACKER";

    private TrackerToggleKey _key = null!;
    private TMP_Text _icon = null!;
    private LocalizedText _label = null!;
    private Color _defaultLabelColor;
    private Color _changedLabelColor;

    /// <summary>Adds the row the first time the page opens; afterwards shows the key, saving one just picked.</summary>
    public static void AttachTo(PauseMenuControlsPage page, TrackerToggleKey key)
    {
        RegisterLabel();
        TrackerKeyRow? row = page.controlsMenuButtonsParent.GetComponentsInChildren<TrackerKeyRow>(includeInactive: true).FirstOrDefault();
        if (row == null)
            row = Create(page, key);
        if (row == null)
            return;
        // Back from the game's rebinding page with a new key.
        if (key.SaveIfChanged())
            Plugin.Hud.ShowToast(ModText.Format(ModTextKey.ControlsKeySet, key.DisplayName));
        row.ShowKey();
    }

    private static TrackerKeyRow? Create(PauseMenuControlsPage page, TrackerToggleKey key)
    {
        PauseMenuRebindButton? source = LastRowOfFirstColumn(page);
        if (source == null)
        {
            Plugin.Log.LogWarning("The Controls page has no rebinding row to copy; the tracker key row is left out.");
            return null;
        }
        GameObject clone = Object.Instantiate(source.gameObject, source.transform.parent);
        clone.name = RowName;
        clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

        // The copied PauseMenuRebindButton would drive the game's action; keep its parts, drop it.
        PauseMenuRebindButton copied = clone.GetComponent<PauseMenuRebindButton>();
        Button rebind = copied.rebindButton;
        Button reset = copied.resetButton;
        LocalizedText label = copied.inputDescriptionText;
        GameObject duplicateWarning = copied.warning;
        Color defaultColor = copied.defaultTextColor;
        Color changedColor = copied.overriddenTextColor;
        Object.DestroyImmediate(copied);
        InputIcon icon = clone.GetComponentInChildren<InputIcon>(includeInactive: true);
        TMP_Text iconText = icon.GetComponent<TMP_Text>();
        // InputIcon shows one of the game's actions; the row writes the mod's key itself.
        Object.DestroyImmediate(icon);

        // A fresh event drops the copied listeners, the inspector's ones included.
        rebind.onClick = new Button.ButtonClickedEvent();
        reset.onClick = new Button.ButtonClickedEvent();
        duplicateWarning.SetActive(false);
        label.index = LabelKey;
        label.tmp.text = LocalizedText.GetText(LabelKey);

        TrackerKeyRow row = clone.AddComponent<TrackerKeyRow>();
        row._key = key;
        row._icon = iconText;
        row._label = label;
        row._defaultLabelColor = defaultColor;
        row._changedLabelColor = changedColor;
        rebind.onClick.AddListener(() => row.Rebind(page));
        reset.onClick.AddListener(row.ResetKey);
        return row;
    }

    // The rows sit in columns under controlsMenuButtonsParent; the first column ends with room below it.
    private static PauseMenuRebindButton? LastRowOfFirstColumn(PauseMenuControlsPage page)
    {
        PauseMenuRebindButton[] rows = page.controlsMenuButtonsParent.GetComponentsInChildren<PauseMenuRebindButton>(includeInactive: true);
        if (rows.Length == 0)
            return null;
        Transform firstColumn = rows[0].transform.parent;
        return rows.Where(row => row.transform.parent == firstColumn).OrderBy(row => row.transform.GetSiblingIndex()).Last();
    }

    /// <summary>The game's table is rebuilt on a language reload, so the label is put back each time.</summary>
    private static void RegisterLabel()
    {
        int languages = Enum.GetValues(typeof(LocalizedText.Language)).Length;
        string english = ModText.In(ModTextKey.ControlsToggleTracker, ModText.ModLanguage.English);
        var texts = Enumerable.Repeat(english, languages).ToList();
        texts[(int)LocalizedText.Language.French] = ModText.In(ModTextKey.ControlsToggleTracker, ModText.ModLanguage.French);
        LocalizedText.mainTable[LabelKey] = texts;
    }

    // What PauseMenuRebindButton.OnRebindClicked does, with the mod's action and the keyboard forced:
    // the tracker key has no gamepad binding.
    private void Rebind(PauseMenuControlsPage page)
    {
        PauseMenuRebindKeyPage.inputAction = _key.Action;
        PauseMenuRebindKeyPage.inputLocIndex = LabelKey;
        PauseMenuRebindKeyPage.forcedInputScheme = InputScheme.KeyboardMouse;
        // The page's own handler, found as PauseMenuControlsPage.InitButtons finds it.
        page.GetComponentInParent<UIPageHandler>().TransistionToPage<PauseMenuRebindKeyPage>();
    }

    private void ResetKey()
    {
        _key.ResetToDefault();
        ShowKey();
        Plugin.Hud.ShowToast(ModText.Format(ModTextKey.ControlsKeySet, _key.DisplayName));
    }

    private void ShowKey()
    {
        InputSpriteData sprites = SingletonAsset<InputSpriteData>.Instance;
        _icon.spriteAsset = sprites.keyboardSprites;
        _icon.text = sprites.GetSpriteTagFromInputPathKeyboard(_key.Path);
        // The game marks a row whose key the player changed with another label colour.
        _label.tmp.color = _key.IsDefault ? _defaultLabelColor : _changedLabelColor;
    }
}

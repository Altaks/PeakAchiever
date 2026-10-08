using System.Collections.Generic;
using System.Linq;
using PeakAchiever.Game;
using PeakAchiever.Hud;
using PeakAchiever.Localization;
using PeakAchiever.Pinning;
using PeakAchiever.Team;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PeakAchiever.PauseMenu;

/// <summary>
/// The host's team pins, on the badges page in a multiplayer game: every badge worth pinning for the whole
/// team, those nobody has first, each with a pin toggle. Made of the <see cref="PaperKit"/> parts and opened
/// from a ribbon under the Statistics one, shown to the host only.
/// </summary>
internal sealed class TeamPanel
{
    // Sizes in reference pixels of the 1920x1080 canvas.
    private const float SheetWidth = 820f;
    private const float IconSize = 44f;
    private const float PinSize = 36f;
    private const float PinGlyphSize = 18f;
    private const float NameFontSize = 21f;
    private const float SubFontSize = 15f;
    private const float GroupFontSize = 16f;
    private const float TagFontSize = 16f;
    private const float SummaryFontSize = 17f;
    private const float SubAlpha = 0.8f;
    private const float GroupAlpha = 0.7f;
    // Players join and earn badges while the panel is open; this often it checks for a change.
    private const float RecheckSeconds = 2f;
    private const string OpenButtonName = "PeakAchiever.ScoutsButton";
    private const string OpenButtonLabelKey = "PEAKACHIEVER_SCOUTS";
    private const string RootName = "PeakAchiever.Scouts";
    private const string NameSeparator = ", ";

    // The ribbon orange darkened to read on the paper at 5.5:1 (#A8470F there is 4.3:1).
    private static readonly Color TagInk = new Color32(0x8C, 0x3B, 0x0D, 0xFF);

    private PauseMenuAccoladesPage? _page;
    private BadgeManager? _badges;
    private GameObject? _root;
    private GameObject? _openButton;
    private PaperKit _kit = null!;
    private TMP_Text _summary = null!;
    private Transform _list = null!;
    private TMP_Text _unknown = null!;
    private string _shown = "";
    // Each row's pin toggle, recoloured in place when the pins change: rebuilding every row costs a hitch.
    private readonly Dictionary<ACHIEVEMENTTYPE, (Image Ring, Image Glyph)> _toggles = [];
    private float _nextRecheck;

    /// <summary>Follows the badges page; the first time, builds the panel and its ribbon.</summary>
    public void Watch(PauseMenuAccoladesPage page)
    {
        _page = page;
        GameTextTable.Register(OpenButtonLabelKey, ModTextKey.TeamButton);
        if (_root == null)
        {
            if (PaperKit.Read(page, "team") is not { } kit)
                return;
            _kit = kit;
            _badges = page.GetComponentInChildren<BadgeManager>(includeInactive: true);
            Build(page);
            Transform column = page.backButton.transform.parent;
            // == null, not ??: Unity objects override the null check.
            Transform stats = column.Find(StatsPanel.OpenButtonName);
            Transform after = stats == null ? page.backButton.transform : stats;
            _openButton = PaperKit.OpenButton(page, after, OpenButtonName, OpenButtonLabelKey, Open);
            _openButton.SetActive(TeamSync.IsHost);
            PaperKit.ArrangeButtons(page);
        }
        ShowOpenButton();
    }

    private void ShowOpenButton()
    {
        if (_openButton == null || _page == null || _openButton.activeSelf == TeamSync.IsHost)
            return;
        _openButton.SetActive(TeamSync.IsHost);
        PaperKit.ArrangeButtons(_page);
    }

    /// <summary>Called every frame by the overlay.</summary>
    public void Tick()
    {
        ShowOpenButton();
        if (_root == null || !_root.activeSelf)
            return;
        if (_page == null || !_page.gameObject.activeInHierarchy || !GUIManager.InPauseMenu || !TeamSync.IsHost)
        {
            _root.SetActive(false);
            return;
        }
        if (Time.unscaledTime < _nextRecheck)
            return;
        _nextRecheck = Time.unscaledTime + RecheckSeconds;
        Fill();
    }

    private void Build(PauseMenuAccoladesPage page)
    {
        _root = PaperKit.Veil(page.transform, RootName);
        Transform column = PaperKit.Column(_root.transform);
        Transform sheet = _kit.Sheet(column, SheetWidth);
        _summary = _kit.Text(sheet, "Summary", SummaryFontSize, PaperKit.PaperInk, TextAlignmentOptions.Center);

        // A fixed height, so the panel does not jump as scouts join and earn badges.
        _list = PaperKit.ScrollList(sheet, "List", PaperKit.MaxListHeight);

        _unknown = _kit.Text(sheet, "WithoutTheMod", SubFontSize, PaperKit.PaperInk, TextAlignmentOptions.Left);
        _kit.Title(sheet, ModText.Get(ModTextKey.TeamTitle));

        Transform actions = PaperKit.Row(column, "Actions", TextAnchor.MiddleCenter);
        PaperKit.Ribbon(_kit.RedRibbon, actions, "Close", ModText.Get(ModTextKey.StatsClose), Close);
        _root.SetActive(false);
    }

    private void Open()
    {
        if (_root == null || !TeamSync.IsHost)
            return;
        _root.transform.SetAsLastSibling();
        _root.SetActive(true);
        _shown = "";
        Fill();
    }

    private void Close()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    /// <summary>Recolours the pin toggles; rebuilds the rows only when what the scouts have changed.</summary>
    private void Fill()
    {
        IReadOnlyList<Scout> scouts = TeamSync.Scouts;
        IReadOnlyList<ACHIEVEMENTTYPE> pins = TeamSync.TeamPins;
        _summary.text = ModText.Format(ModTextKey.TeamSummary, scouts.Count, pins.Count, Plugin.Pins.TeamCapacity.Value);
        string state = string.Join("|", scouts.Select(scout => scout.Name + ":" + (scout.Earned?.Count ?? -1)));
        if (state != _shown)
        {
            _shown = state;
            Rebuild(scouts);
        }
        foreach (KeyValuePair<ACHIEVEMENTTYPE, (Image Ring, Image Glyph)> toggle in _toggles)
            Paint(toggle.Value.Ring, toggle.Value.Glyph, pins.Contains(toggle.Key));
    }

    private void Rebuild(IReadOnlyList<Scout> scouts)
    {
        // Destroy, not DestroyImmediate: the old rows go at the end of the frame, without stalling it.
        for (int i = _list.childCount - 1; i >= 0; i--)
        {
            GameObject old = _list.GetChild(i).gameObject;
            old.transform.SetParent(null, worldPositionStays: false);
            Object.Destroy(old);
        }
        _toggles.Clear();
        IReadOnlyList<TeamRow> rows = TeamRanking.Rank(Candidates(), scouts);
        AddGroup(ModTextKey.TeamGroupNobody, rows.Where(row => row.NobodyHasIt));
        AddGroup(ModTextKey.TeamGroupSome, rows.Where(row => !row.NobodyHasIt));
        string[] withoutTheMod = scouts.Where(scout => scout.Earned == null).Select(scout => scout.Name).ToArray();
        _unknown.gameObject.SetActive(withoutTheMod.Length > 0);
        _unknown.text = ModText.Format(ModTextKey.TeamWithoutTheMod, string.Join(NameSeparator, withoutTheMod));
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_root!.transform.GetChild(0));
    }

    // The page's badges, in its order; a secret the host has not earned stays out, so nothing is given away.
    private IEnumerable<ACHIEVEMENTTYPE> Candidates() =>
        _badges == null
            ? []
            : _badges.badgeData.Where(data => data != null && !(data.secret && data.IsLocked)).Select(data => data.linkedAchievement);

    private void AddGroup(ModTextKey title, IEnumerable<TeamRow> rows)
    {
        TeamRow[] shown = rows.ToArray();
        if (shown.Length == 0)
            return;
        var ink = new Color(PaperKit.PaperInk.r, PaperKit.PaperInk.g, PaperKit.PaperInk.b, GroupAlpha);
        _kit.Text(_list, "Group", GroupFontSize, ink, TextAlignmentOptions.Left, ModText.Get(title).ToUpperInvariant());
        foreach (TeamRow row in shown)
        {
            AddRow(row);
            _kit.Dots(_list);
        }
    }

    private void AddRow(TeamRow row)
    {
        BadgePresentation badge = BadgeCatalog.Present(row.Badge);
        Transform line = PaperKit.Row(_list, "Row", TextAnchor.MiddleLeft);

        GameObject icon = UiFactory.Create("Icon", line);
        UiFactory.SetFixedSize(icon, IconSize, IconSize);
        RawImage image = icon.AddComponent<RawImage>();
        image.texture = badge.Icon;
        image.raycastTarget = false;

        GameObject words = UiFactory.Create("Words", line);
        VerticalLayoutGroup stack = words.AddComponent<VerticalLayoutGroup>();
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;
        words.AddComponent<LayoutElement>().flexibleWidth = 1f;
        _kit.Text(words.transform, "Name", NameFontSize, PaperKit.PaperInk, TextAlignmentOptions.Left, badge.Name);
        string sub = row.NobodyHasIt ? badge.Description : ModText.Format(ModTextKey.TeamMissingFor, string.Join(NameSeparator, row.MissingFor));
        var soft = new Color(PaperKit.PaperInk.r, PaperKit.PaperInk.g, PaperKit.PaperInk.b, SubAlpha);
        _kit.Text(words.transform, "Sub", SubFontSize, soft, TextAlignmentOptions.Left, sub);

        string tag = row.NobodyHasIt ? ModText.Get(ModTextKey.TeamRecommended) : ModText.Format(ModTextKey.TeamHaveIt, row.EarnedBy, row.Known);
        TMP_Text tagText = _kit.Text(line, "Tag", TagFontSize, TagInk, TextAlignmentOptions.Right, tag);
        tagText.textWrappingMode = TextWrappingModes.NoWrap;

        PinToggle(line, row.Badge);
    }

    // The mod's own pin marker, filled when the badge is pinned for the team.
    private void PinToggle(Transform line, ACHIEVEMENTTYPE badge)
    {
        if (Plugin.Hud.Style is not { } style)
            return;
        GameObject toggle = UiFactory.Create("Pin", line);
        UiFactory.SetFixedSize(toggle, PinSize, PinSize);
        Image ring = UiFactory.AddImage(toggle, style.Circle, PaperKit.PaperInk);
        ring.raycastTarget = true;
        GameObject glyph = UiFactory.Create("Glyph", toggle.transform);
        UiFactory.PinToCorner(glyph, new Vector2(0.5f, 0.5f), Vector2.zero, PinGlyphSize);
        Image mark = UiFactory.AddImage(glyph, style.Pin, _kit.Paper.color);
        mark.preserveAspect = true;
        _toggles[badge] = (ring, mark);
        Button button = toggle.AddComponent<Button>();
        button.targetGraphic = ring;
        button.onClick.AddListener(() => Toggle(badge));
    }

    private void Paint(Image ring, Image glyph, bool pinned)
    {
        ring.color = pinned ? HudStyle.ProgressFill : PaperKit.PaperInk;
        glyph.color = pinned ? HudStyle.PinMarkerInk : _kit.Paper.color;
    }

    private void Toggle(ACHIEVEMENTTYPE badge)
    {
        var board = new PinBoard(TeamSync.TeamPins, Plugin.Pins.TeamCapacity.Value);
        switch (board.Toggle(badge, isEarned: false, MapCatalog.Compatibility))
        {
            case PinToggleOutcome.Pinned:
            case PinToggleOutcome.Unpinned:
                TeamSync.SetTeamPins(board.Pins);
                Plugin.Hud.RequestRefresh();
                Fill();
                break;
            case PinToggleOutcome.RejectedBoardFull:
                Plugin.Hud.ShowToast(ModText.Format(ModTextKey.RefusalTeamFull, Plugin.Pins.TeamCapacity.Value));
                break;
            case PinToggleOutcome.RejectedConflict:
                ACHIEVEMENTTYPE conflict = MapCatalog.Compatibility.FirstConflict(badge, board.Pins)!.Value;
                Plugin.Hud.ShowToast(ModText.Format(ModTextKey.RefusalConflict, BadgeCatalog.Present(conflict).Name));
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(badge), badge, "Unhandled pin outcome.");
        }
    }
}

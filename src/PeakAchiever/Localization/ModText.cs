using System;
using System.Collections.Generic;
using System.Linq;

namespace PeakAchiever.Localization;

internal enum ModTextKey
{
    StatusAttainable,
    StatusHolding,
    StatusAchieved,
    ScopeLifetime,
    LimitRate,
    Eta,
    SecretPlaceholder,
    ReasonBiomeAbsent,
    ReasonBiomeLeft,
    ReasonTookFallDamage,
    ReasonAtePackagedFood,
    ReasonPassedOut,
    ReasonPlacedPermanentItem,
    ReasonRunTooLong,
    ReasonTooMuchHeat,
    ReasonTooMuchCold,
    ReasonTooManySpores,
    ReasonHitByTrap,
    ReasonRunLost,
    HintClickToPin,
    HintClickToUnpin,
    HintClickToPinForAlly,
    RefusalBoardFull,
    RefusalConflict,
    HintConflict,
    HintNotOnTodaysMap,
    HintNotOnThisMap,
    ChecklistNotOnMap,
    HintSuggestedToday,
    HintSuggestedThisMap,
    StatsButton,
    TeamButton,
    TeamTitle,
    TeamSummary,
    TeamGroupNobody,
    TeamGroupSome,
    TeamRecommended,
    TeamHaveIt,
    TeamMissingFor,
    TeamWithoutTheMod,
    RefusalTeamFull,
    ControlsToggleTracker,
    SettingsMaxPins,
    SettingsMaxTeamPins,
    ControlsKeySet,
    StatsTitle,
    StatsAscentFallback,
    StatsColumnBiome,
    StatsColumnTimes,
    StatsColumnMedian,
    StatsColumnBest,
    StatsLayoutTotal,
    StatsEmpty,
    StatsNoLowerAscent,
    StatsNoHigherAscent,
    StatsErase,
    StatsEraseConfirm,
    StatsErased,
    StatsEraseFailed,
    StatsClose,
    AchievementsDisabled,
    ChipAllRuns,
    ChipForAlly,
    StatusYouHaveIt,
    LimitLeft,
    BiomeNow,
    BiomeNext,
    BiomeLater,
    NeedsAll,
    NeedsOneOf,
    NeedsHelps,
    NeedsCount,
    LocatorBell,
    LocatorAntlion,
    LocatorTomb,
    LocatorDistance,
    SettingsMarkers,
    MarkerLabel,
    GroupTeam,
    GroupOwn,
}

/// <summary>
/// The mod's own strings. Badge names and descriptions come from the game's table instead.
/// Each row holds one string per <see cref="LocalizedText.Language"/>, in that enum's order, like the
/// game's own table (LocalizedText.GetText reads list[(int)CURRENT_LANGUAGE], v2.6.b).
/// </summary>
internal static class ModText
{
    // French typography: a no-break space (U+00A0) goes before ':' and '%', as the game's own Frenchify does.
    internal static readonly Dictionary<ModTextKey, string[]> Table = new()
    {
        [ModTextKey.StatusAttainable] = Row("Doable", "Faisable"),
        [ModTextKey.StatusHolding] = Row("Holding so far", "Tenu jusqu'ici"),
        [ModTextKey.StatusAchieved] = Row("Earned", "Obtenu"),
        [ModTextKey.ChipAllRuns] = Row("All runs", "Toutes les parties"),
        [ModTextKey.ChipForAlly] = Row("For an ally", "Pour un allié"),
        [ModTextKey.StatusYouHaveIt] = Row("You have it", "Vous l'avez"),
        [ModTextKey.LimitLeft] = Row("{0}% left", "Reste {0}\u00A0%"),
        [ModTextKey.BiomeNow] = Row("Now, in the {0}", "Maintenant\u00A0: {0}"),
        [ModTextKey.BiomeNext] = Row("{0}, next biome", "{0}, prochain biome"),
        [ModTextKey.BiomeLater] = Row("{0}, in {1} biomes", "{0}, dans {1} biomes"),
        [ModTextKey.NeedsAll] = Row("Needs", "Requis\u00A0"),
        [ModTextKey.NeedsOneOf] = Row("One of", "Un parmi\u00A0"),
        [ModTextKey.NeedsHelps] = Row("Helps", "Utile\u00A0"),
        [ModTextKey.NeedsCount] = Row("×{0}", "×{0}"),
        [ModTextKey.LocatorBell] = Row("Unlit belltower", "Clocher éteint"),
        [ModTextKey.LocatorAntlion] = Row("Antlion", "Fourmilion"),
        [ModTextKey.LocatorTomb] = Row("Mesa tomb", "Tombe du Plateau"),
        [ModTextKey.LocatorDistance] = Row("{0} m", "{0}\u00A0m"),
        [ModTextKey.GroupTeam] = Row("TEAM · SET BY THE HOST", "ÉQUIPE · CHOISIES PAR L'HÔTE"),
        [ModTextKey.GroupOwn] = Row("YOUR PINS", "VOS ÉPINGLES"),
        [ModTextKey.MarkerLabel] = Row("{0} · {1} m", "{0} · {1}\u00A0m"),
        [ModTextKey.SettingsMarkers] = Row("PEAKACHIEVER: ON-SCREEN MARKERS", "PEAKACHIEVER\u00A0: REPÈRES À L'ÉCRAN"),
        [ModTextKey.ScopeLifetime] = Row("LIFETIME", "À VIE"),
        [ModTextKey.Eta] = Row("ETA {0}", "Arrivée estimée : {0}"),
        [ModTextKey.LimitRate] = Row("max {0}% / {1}%", "max {0} % / {1} %"),
        [ModTextKey.SecretPlaceholder] = Row("???", "???"),
        [ModTextKey.ReasonBiomeAbsent] = Row("Impossible: no {0} this run", "Impossible : pas de {0} dans cette run"),
        [ModTextKey.ReasonBiomeLeft] = Row("Impossible: {0} left behind", "Impossible : {0} déjà quitté"),
        [ModTextKey.ReasonTookFallDamage] = Row("Impossible: fall damage taken", "Impossible : dégâts de chute subis"),
        [ModTextKey.ReasonAtePackagedFood] = Row("Impossible: packaged food eaten", "Impossible : nourriture emballée mangée"),
        [ModTextKey.ReasonPassedOut] = Row("Impossible: you passed out", "Impossible : vous vous êtes évanoui"),
        [ModTextKey.ReasonPlacedPermanentItem] = Row("Impossible: permanent item placed", "Impossible : objet permanent posé"),
        [ModTextKey.ReasonRunTooLong] = Row("Impossible: over one hour", "Impossible : plus d'une heure écoulée"),
        [ModTextKey.ReasonTooMuchHeat] = Row("Impossible: too much heat taken", "Impossible : trop de chaleur subie"),
        [ModTextKey.ReasonTooMuchCold] = Row("Impossible: too much cold taken", "Impossible : trop de froid subi"),
        [ModTextKey.ReasonTooManySpores] = Row("Impossible: too many spores taken", "Impossible : trop de spores subies"),
        [ModTextKey.ReasonRunLost] = Row("Impossible: run lost", "Impossible : run perdue"),
        [ModTextKey.ReasonHitByTrap] = Row("Impossible: hit by a trap", "Impossible : touché par un piège"),
        [ModTextKey.HintClickToPin] = Row("Click: pin to tracker", "Clic : épingler au traqueur"),
        [ModTextKey.HintClickToPinForAlly] = Row("Click: pin to help an ally earn it", "Clic : épingler pour aider un allié"),
        [ModTextKey.HintClickToUnpin] = Row("Click: unpin from tracker", "Clic : retirer du traqueur"),
        [ModTextKey.RefusalBoardFull] =
        Row("{0} badges pinned at most. Unpin one, or raise the limit in Settings, General.", "{0} distinctions épinglées au maximum. Retirez-en une, ou augmentez la limite dans Paramètres, Général."),
        [ModTextKey.RefusalConflict] =
        Row("Can't be earned in the same run as {0}: no map has both their biomes. Unpin it first.", "Impossible dans la même run que {0} : aucune carte n'a leurs deux biomes. Retirez-la d'abord."),
        [ModTextKey.HintConflict] =
        Row("Conflicts with {0}: no map has both their biomes", "Incompatible avec {0} : aucune carte n'a leurs deux biomes"),
        [ModTextKey.HintNotOnTodaysMap] = Row("Not on today's map ({0})", "Pas sur la carte du jour ({0})"),
        [ModTextKey.HintNotOnThisMap] = Row("Not on this map ({0})", "Pas sur cette carte ({0})"),
        [ModTextKey.HintSuggestedToday] = Row("Suggested: doable on today's map", "Suggéré : faisable sur la carte du jour"),
        [ModTextKey.HintSuggestedThisMap] = Row("Suggested: doable on this map", "Suggéré : faisable sur cette carte"),
        [ModTextKey.ChecklistNotOnMap] = Row("Not seen on this map", "Pas vu sur cette carte"),
        [ModTextKey.ControlsToggleTracker] = Row("PeakAchiever: show / hide the tracker", "PeakAchiever : afficher / masquer le traqueur"),
        [ModTextKey.SettingsMaxPins] = Row("PEAKACHIEVER: MAX PINNED BADGES", "PEAKACHIEVER : DISTINCTIONS ÉPINGLÉES MAX."),
        [ModTextKey.SettingsMaxTeamPins] = Row("PEAKACHIEVER: MAX TEAM PINS", "PEAKACHIEVER : ÉPINGLES SCOUTS MAX."),
        [ModTextKey.ControlsKeySet] = Row("Tracker key set to {0}.", "Touche du traqueur : {0}."),
        [ModTextKey.TeamButton] = Row("Scouts", "Scouts"),
        [ModTextKey.TeamTitle] = Row("SCOUTS", "SCOUTS"),
        [ModTextKey.TeamSummary] = Row("{0} scouts in the game · {1} / {2} pinned for the team", "{0} scouts dans la partie · {1} / {2} épinglées pour l'équipe"),
        [ModTextKey.TeamGroupNobody] = Row("Nobody has it", "Personne ne l'a"),
        [ModTextKey.TeamGroupSome] = Row("Already earned by some", "Déjà obtenues par certains"),
        [ModTextKey.TeamRecommended] = Row("Recommended", "Recommandée"),
        [ModTextKey.TeamHaveIt] = Row("{0} / {1} have it", "{0} / {1} l'ont"),
        [ModTextKey.TeamMissingFor] = Row("Missing for {0}", "Il manque à {0}"),
        [ModTextKey.TeamWithoutTheMod] = Row("Without the mod: {0}. Their badges are unknown, and they will not see the team pins.", "Sans le mod : {0}. Leurs distinctions sont inconnues, et ils ne verront pas les épingles Scouts."),
        [ModTextKey.RefusalTeamFull] = Row("{0} team pins at most. Unpin one, or raise the limit in Settings, General.", "{0} épingles Scouts au maximum. Retirez-en une, ou augmentez la limite dans Paramètres, Général."),
        [ModTextKey.StatsButton] = Row("Statistics", "Statistiques"),
        [ModTextKey.StatsTitle] = Row("STATISTICS", "STATISTIQUES"),
        [ModTextKey.StatsAscentFallback] = Row("Ascent {0}", "Ascension {0}"),
        [ModTextKey.StatsColumnBiome] = Row("Biome", "Biome"),
        [ModTextKey.StatsColumnTimes] = Row("Times", "Fois"),
        [ModTextKey.StatsColumnMedian] = Row("Median", "Médiane"),
        [ModTextKey.StatsColumnBest] = Row("Best", "Meilleur"),
        [ModTextKey.StatsLayoutTotal] = Row("Total ({0})", "Total ({0})"),
        [ModTextKey.StatsEmpty] = Row("No biome finished at this ascent yet.", "Aucun biome terminé à cette ascension pour l'instant."),
        [ModTextKey.StatsNoLowerAscent] =
        Row("No lower ascent has biome times yet: finish a biome at another ascent to see it here.", "Aucune ascension plus basse n'a encore de temps : terminez un biome à une autre ascension pour la voir ici."),
        [ModTextKey.StatsNoHigherAscent] =
        Row("No higher ascent has biome times yet: finish a biome at another ascent to see it here.", "Aucune ascension plus haute n'a encore de temps : terminez un biome à une autre ascension pour la voir ici."),
        [ModTextKey.StatsErase] = Row("Erase this ascent", "Effacer cette ascension"),
        [ModTextKey.StatsEraseConfirm] = Row("Click again to erase {0}", "Cliquez encore pour effacer {0}"),
        [ModTextKey.StatsErased] = Row("{0}: biome times erased.", "{0} : temps des biomes effacés."),
        [ModTextKey.StatsEraseFailed] = Row("Could not erase the file; see LogOutput.log.", "Impossible d'effacer le fichier : voir LogOutput.log."),
        [ModTextKey.StatsClose] = Row("Close", "Fermer"),
        [ModTextKey.AchievementsDisabled] = Row("Achievements are disabled for this run", "Succès désactivés pour cette run"),
    };

    public static string Get(ModTextKey key) => In(key, LocalizedText.CURRENT_LANGUAGE);

    /// <summary>The string in a given language.</summary>
    public static string In(ModTextKey key, LocalizedText.Language language) => Table[key][(int)language];

    public static string Format(ModTextKey key, params object[] arguments) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Get(key), arguments);

    // The languages without their own string yet read English, as they did before the table followed the game's enum.
    private static string[] Row(string english, string french)
    {
        var row = Enumerable.Repeat(english, Enum.GetValues(typeof(LocalizedText.Language)).Length).ToArray();
        row[(int)LocalizedText.Language.French] = french;
        return row;
    }
}

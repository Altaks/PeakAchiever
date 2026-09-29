using System.Collections.Generic;

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
    ControlsToggleTracker,
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
}

/// <summary>
/// The mod's own strings. Badge names and descriptions come from the game's table instead.
/// Adding a language means one <see cref="ModLanguage"/> member and one more string on each row.
/// </summary>
internal static class ModText
{
    internal enum ModLanguage
    {
        English,
        French,
    }

    // French typography: a no-break space (U+00A0) goes before ':' and '%', as the game's own Frenchify does.
    internal static readonly Dictionary<ModTextKey, string[]> Table = new()
    {
        [ModTextKey.StatusAttainable] = ["Doable", "Faisable"],
        [ModTextKey.StatusHolding] = ["Holding so far", "Tenu jusqu'ici"],
        [ModTextKey.StatusAchieved] = ["Earned", "Obtenu"],
        [ModTextKey.ScopeLifetime] = ["LIFETIME", "À VIE"],
        [ModTextKey.Eta] = ["ETA {0}", "Arrivée estimée : {0}"],
        [ModTextKey.LimitRate] = ["max {0}% / {1}%", "max {0} % / {1} %"],
        [ModTextKey.SecretPlaceholder] = ["???", "???"],
        [ModTextKey.ReasonBiomeAbsent] = ["Impossible: no {0} this run", "Impossible : pas de {0} dans cette run"],
        [ModTextKey.ReasonBiomeLeft] = ["Impossible: {0} left behind", "Impossible : {0} déjà quitté"],
        [ModTextKey.ReasonTookFallDamage] = ["Impossible: fall damage taken", "Impossible : dégâts de chute subis"],
        [ModTextKey.ReasonAtePackagedFood] = ["Impossible: packaged food eaten", "Impossible : nourriture emballée mangée"],
        [ModTextKey.ReasonPassedOut] = ["Impossible: you passed out", "Impossible : vous vous êtes évanoui"],
        [ModTextKey.ReasonPlacedPermanentItem] = ["Impossible: permanent item placed", "Impossible : objet permanent posé"],
        [ModTextKey.ReasonRunTooLong] = ["Impossible: over one hour", "Impossible : plus d'une heure écoulée"],
        [ModTextKey.ReasonTooMuchHeat] = ["Impossible: too much heat taken", "Impossible : trop de chaleur subie"],
        [ModTextKey.ReasonTooMuchCold] = ["Impossible: too much cold taken", "Impossible : trop de froid subi"],
        [ModTextKey.ReasonTooManySpores] = ["Impossible: too many spores taken", "Impossible : trop de spores subies"],
        [ModTextKey.ReasonRunLost] = ["Impossible: run lost", "Impossible : run perdue"],
        [ModTextKey.ReasonHitByTrap] = ["Impossible: hit by a trap", "Impossible : touché par un piège"],
        [ModTextKey.HintClickToPin] = ["Click: pin to tracker", "Clic : épingler au traqueur"],
        [ModTextKey.HintClickToPinForAlly] = ["Click: pin to help an ally earn it", "Clic : épingler pour aider un allié"],
        [ModTextKey.HintClickToUnpin] = ["Click: unpin from tracker", "Clic : retirer du traqueur"],
        [ModTextKey.RefusalBoardFull] =
        [
            "{0} badges pinned at most. Unpin one, or raise the limit in the config.",
            "{0} distinctions épinglées au maximum. Retirez-en une, ou augmentez la limite dans la config.",
        ],
        [ModTextKey.RefusalConflict] =
        [
            "Can't be earned in the same run as {0}: no map has both their biomes. Unpin it first.",
            "Impossible dans la même run que {0} : aucune carte n'a leurs deux biomes. Retirez-la d'abord.",
        ],
        [ModTextKey.HintConflict] =
        [
            "Conflicts with {0}: no map has both their biomes",
            "Incompatible avec {0} : aucune carte n'a leurs deux biomes",
        ],
        [ModTextKey.HintNotOnTodaysMap] = ["Not on today's map ({0})", "Pas sur la carte du jour ({0})"],
        [ModTextKey.HintNotOnThisMap] = ["Not on this map ({0})", "Pas sur cette carte ({0})"],
        [ModTextKey.HintSuggestedToday] = ["Suggested: doable on today's map", "Suggéré : faisable sur la carte du jour"],
        [ModTextKey.HintSuggestedThisMap] = ["Suggested: doable on this map", "Suggéré : faisable sur cette carte"],
        [ModTextKey.ChecklistNotOnMap] = ["Not seen on this map", "Pas vu sur cette carte"],
        [ModTextKey.ControlsToggleTracker] = ["PeakAchiever: show / hide the tracker", "PeakAchiever : afficher / masquer le traqueur"],
        [ModTextKey.ControlsKeySet] = ["Tracker key set to {0}.", "Touche du traqueur : {0}."],
        [ModTextKey.StatsButton] = ["Statistics", "Statistiques"],
        [ModTextKey.StatsTitle] = ["STATISTICS", "STATISTIQUES"],
        [ModTextKey.StatsAscentFallback] = ["Ascent {0}", "Ascension {0}"],
        [ModTextKey.StatsColumnBiome] = ["Biome", "Biome"],
        [ModTextKey.StatsColumnTimes] = ["Times", "Fois"],
        [ModTextKey.StatsColumnMedian] = ["Median", "Médiane"],
        [ModTextKey.StatsColumnBest] = ["Best", "Meilleur"],
        [ModTextKey.StatsLayoutTotal] = ["Total ({0})", "Total ({0})"],
        [ModTextKey.StatsEmpty] = ["No biome finished at this ascent yet.", "Aucun biome terminé à cette ascension pour l'instant."],
        [ModTextKey.StatsNoLowerAscent] =
        [
            "No lower ascent has biome times yet: finish a biome at another ascent to see it here.",
            "Aucune ascension plus basse n'a encore de temps : terminez un biome à une autre ascension pour la voir ici.",
        ],
        [ModTextKey.StatsNoHigherAscent] =
        [
            "No higher ascent has biome times yet: finish a biome at another ascent to see it here.",
            "Aucune ascension plus haute n'a encore de temps : terminez un biome à une autre ascension pour la voir ici.",
        ],
        [ModTextKey.StatsErase] = ["Erase this ascent", "Effacer cette ascension"],
        [ModTextKey.StatsEraseConfirm] = ["Click again to erase {0}", "Cliquez encore pour effacer {0}"],
        [ModTextKey.StatsErased] = ["{0}: biome times erased.", "{0} : temps des biomes effacés."],
        [ModTextKey.StatsEraseFailed] = ["Could not erase the file; see LogOutput.log.", "Impossible d'effacer le fichier : voir LogOutput.log."],
        [ModTextKey.StatsClose] = ["Close", "Fermer"],
        [ModTextKey.AchievementsDisabled] = ["Achievements are disabled for this run", "Succès désactivés pour cette run"],
    };

    public static string Get(ModTextKey key) => Table[key][(int)CurrentLanguage];

    /// <summary>The string in a given language, for texts handed to the game's own table.</summary>
    public static string In(ModTextKey key, ModLanguage language) => Table[key][(int)language];

    public static string Format(ModTextKey key, params object[] arguments) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Get(key), arguments);

    private static ModLanguage CurrentLanguage =>
        LocalizedText.CURRENT_LANGUAGE == LocalizedText.Language.French ? ModLanguage.French : ModLanguage.English;
}

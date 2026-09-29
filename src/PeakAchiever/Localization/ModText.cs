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
    ReasonNotSolo,
    ReasonTooMuchHeat,
    ReasonTooMuchCold,
    ReasonTooManySpores,
    ReasonHitByTrap,
    HintClickToPin,
    HintClickToUnpin,
    RefusalBoardFull,
    RefusalAlreadyEarned,
    RefusalConflict,
    HintConflict,
    HintNotOnTodaysMap,
    HintNotOnThisMap,
    ChecklistNotOnMap,
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
        [ModTextKey.ReasonNotSolo] = ["Impossible: not climbing alone", "Impossible : vous n'êtes pas seul"],
        [ModTextKey.ReasonTooMuchHeat] = ["Impossible: too much heat taken", "Impossible : trop de chaleur subie"],
        [ModTextKey.ReasonTooMuchCold] = ["Impossible: too much cold taken", "Impossible : trop de froid subi"],
        [ModTextKey.ReasonTooManySpores] = ["Impossible: too many spores taken", "Impossible : trop de spores subies"],
        [ModTextKey.ReasonHitByTrap] = ["Impossible: hit by a trap", "Impossible : touché par un piège"],
        [ModTextKey.HintClickToPin] = ["Click: pin to tracker", "Clic : épingler au traqueur"],
        [ModTextKey.HintClickToUnpin] = ["Click: unpin from tracker", "Clic : retirer du traqueur"],
        [ModTextKey.RefusalBoardFull] =
        [
            "{0} badges pinned at most. Unpin one, or raise the limit in the config.",
            "{0} distinctions épinglées au maximum. Retirez-en une, ou augmentez la limite dans la config.",
        ],
        [ModTextKey.RefusalAlreadyEarned] = ["Already earned, nothing left to chase.", "Déjà obtenue, plus rien à viser."],
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
        [ModTextKey.ChecklistNotOnMap] = ["Not seen on this map", "Pas vu sur cette carte"],
        [ModTextKey.AchievementsDisabled] = ["Achievements are disabled for this run", "Succès désactivés pour cette run"],
    };

    public static string Get(ModTextKey key) => Table[key][(int)CurrentLanguage];

    public static string Format(ModTextKey key, params object[] arguments) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Get(key), arguments);

    private static ModLanguage CurrentLanguage =>
        LocalizedText.CURRENT_LANGUAGE == LocalizedText.Language.French ? ModLanguage.French : ModLanguage.English;
}
